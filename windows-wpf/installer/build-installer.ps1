#Requires -Version 5.1
param(
    [string]$Version = "1.0.0",
    [string]$PublishDir = "..\publish",
    [string]$WixVersion = "4.0.6"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$toolPath = Join-Path $repoRoot ".tools\wix4"
$wixExe = Join-Path $toolPath "wix.exe"
$publishDirPath = if ([System.IO.Path]::IsPathRooted($PublishDir)) { $PublishDir } else { Join-Path $PSScriptRoot $PublishDir }
$harvestPath = Join-Path $PSScriptRoot "HarvestedWorkspace.wxs"

function Get-StableWixId {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prefix,
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.ToLowerInvariant())
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha.ComputeHash($bytes)
    }
    finally {
        $sha.Dispose()
    }
    $hash = ([System.BitConverter]::ToString($hashBytes)).Replace("-", "").Substring(0, 16)
    return "{0}_{1}" -f $Prefix, $hash
}

function Get-StableGuid {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.ToLowerInvariant())
    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $hashBytes = $md5.ComputeHash($bytes)
    }
    finally {
        $md5.Dispose()
    }

    return ([System.Guid]::new($hashBytes)).ToString()
}

function Escape-Xml {
    param([string]$Value)
    return [System.Security.SecurityElement]::Escape($Value)
}

function Add-DirectoryContent {
    param(
        [Parameter(Mandatory = $true)]
        [System.Text.StringBuilder]$Builder,
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$RootPublishDir,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[string]]$ComponentIds,
        [Parameter(Mandatory = $true)]
        [int]$IndentLevel
    )

    $indent = ("  " * $IndentLevel)
    $files = Get-ChildItem $SourceDir -File | Sort-Object Name
    if ($SourceDir -eq $RootPublishDir) {
        $files = $files | Where-Object { $_.Name -ne "MetaSkillStudio.exe" }
    }

    if ($files.Count -gt 0) {
        $componentId = Get-StableWixId -Prefix "cmp" -Value $SourceDir
        $componentGuid = Get-StableGuid -Value $SourceDir
        $ComponentIds.Add($componentId) | Out-Null
        [void]$Builder.AppendLine("$indent<Component Id=`"$componentId`" Guid=`"$componentGuid`">")

        $isFirstFile = $true
        foreach ($file in $files) {
            $fileId = Get-StableWixId -Prefix "fil" -Value $file.FullName
            $keyPath = if ($isFirstFile) { " KeyPath=`"yes`"" } else { "" }
            $escapedName = Escape-Xml $file.Name
            $escapedSource = Escape-Xml $file.FullName
            [void]$Builder.AppendLine("$indent  <File Id=`"$fileId`" Name=`"$escapedName`" Source=`"$escapedSource`"$keyPath />")
            $isFirstFile = $false
        }

        [void]$Builder.AppendLine("$indent</Component>")
    }

    $directories = Get-ChildItem $SourceDir -Directory | Sort-Object Name
    foreach ($directory in $directories) {
        $directoryId = Get-StableWixId -Prefix "dir" -Value $directory.FullName
        $escapedName = Escape-Xml $directory.Name
        [void]$Builder.AppendLine("$indent<Directory Id=`"$directoryId`" Name=`"$escapedName`">")
        Add-DirectoryContent -Builder $Builder -SourceDir $directory.FullName -RootPublishDir $RootPublishDir -ComponentIds $ComponentIds -IndentLevel ($IndentLevel + 1)
        [void]$Builder.AppendLine("$indent</Directory>")
    }
}

function New-WorkspaceHarvest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$OutputPath
    )

    $componentIds = [System.Collections.Generic.List[string]]::new()
    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.AppendLine('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
    [void]$builder.AppendLine('  <Fragment>')
    [void]$builder.AppendLine('    <DirectoryRef Id="INSTALLFOLDER">')
    Add-DirectoryContent -Builder $builder -SourceDir $SourceDir -RootPublishDir $SourceDir -ComponentIds $componentIds -IndentLevel 3
    [void]$builder.AppendLine('    </DirectoryRef>')
    [void]$builder.AppendLine('  </Fragment>')
    [void]$builder.AppendLine('  <Fragment>')
    [void]$builder.AppendLine('    <ComponentGroup Id="BundledWorkspaceComponents">')
    foreach ($componentId in $componentIds) {
        [void]$builder.AppendLine("      <ComponentRef Id=`"$componentId`" />")
    }
    [void]$builder.AppendLine('    </ComponentGroup>')
    [void]$builder.AppendLine('  </Fragment>')
    [void]$builder.AppendLine('</Wix>')

    Set-Content -Path $OutputPath -Value $builder.ToString() -Encoding UTF8
}

if (-not (Test-Path $publishDirPath)) {
    Write-Error "Publish directory not found at $publishDirPath. Run ..\build-release.ps1 first."
    exit 1
}

$resolvedPublishDir = (Resolve-Path $publishDirPath).Path
$exePath = Join-Path $resolvedPublishDir "MetaSkillStudio.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Executable not found at $exePath. Run ..\build-release.ps1 first."
    exit 1
}

if (-not (Test-Path $wixExe)) {
    dotnet tool install wix --version $WixVersion --tool-path $toolPath
}

& $wixExe extension add "WixToolset.UI.wixext/$WixVersion"

New-WorkspaceHarvest -SourceDir $resolvedPublishDir -OutputPath $harvestPath

$msiVersion = $Version.Split('-')[0]
$msiPath = "MetaSkillStudio-$Version.msi"

Write-Host "Building MSI installer for Meta Skill Studio v$Version..."

Push-Location $PSScriptRoot
try {
    & $wixExe build `
        -arch x64 `
        -ext WixToolset.UI.wixext `
        -d "ProductVersion=$msiVersion" `
        -d "PublishDir=$resolvedPublishDir" `
        -o "$msiPath" `
        "MetaSkillStudio.wxs" `
        "HarvestedWorkspace.wxs"
}
finally {
    Pop-Location
}

if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Success! MSI created: $msiPath"
