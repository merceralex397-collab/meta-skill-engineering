#Requires -Version 5.1
param(
    [string]$Version = "1.0.0",
    [string]$PublishDir = "..\publish"
)

$ErrorActionPreference = "Stop"

# Find WiX
$wixPaths = @(
    "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin",
    "${env:ProgramFiles}\WiX Toolset v3.11\bin",
    "${env:ProgramFiles(x86)}\WiX Toolset v4.0\bin",
    "${env:ProgramFiles}\WiX Toolset v4.0\bin"
)

$wixFound = $false
foreach ($path in $wixPaths) {
    if (Test-Path (Join-Path $path "candle.exe")) {
        $env:PATH = "$path;$env:PATH"
        $wixFound = $true
        break
    }
}

if (-not $wixFound) {
    Write-Error "WiX Toolset not found! Install with: winget install WiXToolset.WiXToolset"
    exit 1
}

# Check executable
$exePath = Join-Path $PublishDir "MetaSkillStudio.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "Executable not found at $exePath. Run build.bat first!"
    exit 1
}

Write-Host "Building MSI installer for Meta Skill Studio v$Version..."

# Compile
& candle.exe -nologo `
    -dProductVersion="$Version" `
    -dPublishDir="$PublishDir" `
    -out "MetaSkillStudio.wixobj" `
    "MetaSkillStudio.wxs"

if ($LASTEXITCODE -ne 0) { exit 1 }

# Link
$msiPath = "MetaSkillStudio-$Version.msi"
& light.exe -nologo -out "$msiPath" -ext WixUIExtension "MetaSkillStudio.wixobj"

if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Success! MSI created: $msiPath"
