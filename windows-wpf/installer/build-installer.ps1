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
$resolvedPublishDir = (Resolve-Path $PublishDir).Path
$exePath = Join-Path $resolvedPublishDir "MetaSkillStudio.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Executable not found at $exePath. Publish the application first."
    exit 1
}

if (-not (Test-Path $wixExe)) {
    dotnet tool install wix --version $WixVersion --tool-path $toolPath
}

& $wixExe extension add "WixToolset.UI.wixext/$WixVersion"

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
        "MetaSkillStudio.wxs"
}
finally {
    Pop-Location
}

if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Success! MSI created: $msiPath"
