#!/usr/bin/env powershell
#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the MSI installer for Meta Skill Studio.
.DESCRIPTION
    Uses WiX Toolset to create a professional Windows Installer package.
#>

[CmdletBinding()]
param(
    [string]$Version = "1.0.0",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent $ScriptDir
$DistDir = Join-Path $ScriptDir "dist"
$InstallerDir = Join-Path $ScriptDir "installer"
$WixObjDir = Join-Path $InstallerDir "obj"

function Write-Header($text) {
    Write-Host "`n=== $text ===" -ForegroundColor Cyan
}

function Test-Command($command) {
    try {
        $null = Get-Command $command -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

# Check for WiX
Write-Header "Checking WiX Toolset"
$wixPaths = @(
    "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin",
    "${env:ProgramFiles}\WiX Toolset v3.11\bin",
    "${env:ProgramFiles(x86)}\WiX Toolset v4.0\bin",
    "${env:ProgramFiles}\WiX Toolset v4.0\bin",
    "${env:LOCALAPPDATA}\Programs\wix"
)

$wixFound = $false
foreach ($path in $wixPaths) {
    if (Test-Path (Join-Path $path "candle.exe")) {
        $env:PATH = "$path;$env:PATH"
        $wixFound = $true
        Write-Host "Found WiX at: $path" -ForegroundColor Green
        break
    }
}

if (-not $wixFound) {
    # Try WiX v4+ (wix.exe)
    if (Test-Command "wix") {
        $wixFound = $true
        Write-Host "Found WiX v4+ (wix.exe)" -ForegroundColor Green
    }
}

if (-not $wixFound) {
    Write-Error @"
WiX Toolset not found! Please install it:
  1. Using winget: winget install WiXToolset.WiXToolset
  2. Or download from: https://wixtoolset.org/
  3. Or use Chocolatey: choco install wixtoolset
"@
    exit 1
}

# Check executable exists
$exePath = Join-Path $DistDir "MetaSkillStudio.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "Executable not found at: $exePath`nRun .\build.ps1 first!"
    exit 1
}

# Clean previous installer builds
if ($Clean) {
    Write-Header "Cleaning previous installer builds"
    Remove-Item -Path $WixObjDir -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Path $WixObjDir -Force | Out-Null

# Generate version info
$versionParts = $Version.Split('.')
$versionHex = "0x$($versionParts[0].PadLeft(4,'0'))$($versionParts[1].PadLeft(2,'0'))$($versionParts[2].PadLeft(2,'0'))00"

# Check if using WiX v3 or v4
$wixV4 = Test-Command "wix"

if ($wixV4) {
    # WiX v4 build
    Write-Header "Building MSI with WiX v4"
    
    $wxsPath = Join-Path $InstallerDir "MetaSkillStudio.wxs"
    $msiPath = Join-Path $DistDir "MetaSkillStudio-$Version.msi"
    
    # Build the MSI
    & wix build -arch x64 -o "$msiPath" "$wxsPath" -d "Version=$Version" -d "ExePath=$exePath"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "WiX build failed with exit code $LASTEXITCODE"
        exit 1
    }
} else {
    # WiX v3 build
    Write-Header "Building MSI with WiX v3"
    
    $wxsPath = Join-Path $InstallerDir "MetaSkillStudio.wxs"
    $wixObj = Join-Path $WixObjDir "MetaSkillStudio.wixobj"
    $msiPath = Join-Path $DistDir "MetaSkillStudio-$Version.msi"
    
    # Compile
    Write-Host "Compiling WiX source..."
    & candle.exe -nologo -out "$wixObj" -dVersion="$Version" -dExePath="$exePath" "$wxsPath"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "WiX compilation failed with exit code $LASTEXITCODE"
        exit 1
    }
    
    # Link
    Write-Host "Linking MSI package..."
    & light.exe -nologo -out "$msiPath" -ext WixUIExtension "$wixObj"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "WiX linking failed with exit code $LASTEXITCODE"
        exit 1
    }
}

# Verify MSI was created
if (-not (Test-Path $msiPath)) {
    Write-Error "Installer build completed but MSI not found at: $msiPath"
    exit 1
}

$msiSize = (Get-Item $msiPath).Length / 1MB
Write-Host "`nInstaller build successful!" -ForegroundColor Green
Write-Host "MSI: $msiPath" -ForegroundColor Yellow
Write-Host "Size: $([math]::Round($msiSize, 2)) MB" -ForegroundColor Yellow

Write-Host "`nDistribution ready!" -ForegroundColor Cyan
Write-Host "You can now distribute: MetaSkillStudio-$Version.msi" -ForegroundColor White
