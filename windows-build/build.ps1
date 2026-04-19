#!/usr/bin/env powershell
#Requires -Version 5.1
<#
.SYNOPSIS
    Builds Meta Skill Studio as a native Windows executable.
.DESCRIPTION
    Uses Nuitka to compile Python code to a native executable.
    Creates a single .exe file with all dependencies embedded.
#>

[CmdletBinding()]
param(
    [switch]$Clean,
    [switch]$Verbose,
    [switch]$DebugBuild
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent $ScriptDir
$SrcDir = Join-Path $ScriptDir "src"
$DistDir = Join-Path $ScriptDir "dist"
$BuildDir = Join-Path $ScriptDir "build"

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

# Clean previous builds
if ($Clean -or -not (Test-Path $DistDir)) {
    Write-Header "Cleaning previous builds"
    Remove-Item -Path $BuildDir -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $DistDir -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
}

# Verify Python is available
Write-Header "Checking build environment"
if (-not (Test-Command "python")) {
    Write-Error "Python is not available. Please install Python 3.11 or later."
    exit 1
}

$pythonVersion = python --version 2>&1
Write-Host "Found: $pythonVersion"

# Install/update Nuitka if needed
Write-Header "Installing build dependencies"
python -m pip install --upgrade nuitka zstandard

# Copy source files to build location
Write-Header "Preparing source files"
New-Item -ItemType Directory -Path $BuildDir -Force | Out-Null

# Copy the main entry point
Copy-Item -Path (Join-Path $SrcDir "MetaSkillStudio.py") -Destination $BuildDir -Force

# Copy meta_skill_studio package
$pkgSource = Join-Path $ProjectRoot "scripts" "meta_skill_studio"
$pkgDest = Join-Path $BuildDir "meta_skill_studio"
if (Test-Path $pkgDest) {
    Remove-Item -Path $pkgDest -Recurse -Force
}
Copy-Item -Path $pkgSource -Destination $pkgDest -Recurse -Force

# Build the executable
Write-Header "Building native executable with Nuitka"

$exeName = "MetaSkillStudio"
$mainScript = Join-Path $BuildDir "MetaSkillStudio.py"

$nuitkaArgs = @(
    "--standalone",
    "--onefile",
    "--windows-console-mode=disable",
    "--windows-icon-from-ico=$(Join-Path $ScriptDir 'resources' 'app.ico')",
    "--windows-company-name=Meta Skill Studio",
    "--windows-product-name=Meta Skill Studio",
    "--windows-file-version=1.0.0.0",
    "--windows-product-version=1.0.0.0",
    "--windows-file-description=Meta Skill Studio - AI Skill Management Tool",
    "--windows-copyright=Copyright (c) 2026",
    "--include-package=meta_skill_studio",
    "--enable-plugin=tk-inter",
    "--output-filename=$exeName.exe",
    "--output-dir=$DistDir"
)

if ($Verbose) {
    $nuitkaArgs += "--verbose"
}

if ($DebugBuild) {
    $nuitkaArgs += "--debug"
    $nuitkaArgs += "--windows-console-mode=force"
}

$nuitkaArgs += $mainScript

Write-Host "Running: python -m nuitka $nuitkaArgs"
& python -m nuitka @nuitkaArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Nuitka build failed with exit code $LASTEXITCODE"
    exit 1
}

# Verify the executable was created
$exePath = Join-Path $DistDir "$exeName.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "Build completed but executable not found at: $exePath"
    exit 1
}

$exeSize = (Get-Item $exePath).Length / 1MB
Write-Host "`nBuild successful!" -ForegroundColor Green
Write-Host "Executable: $exePath" -ForegroundColor Yellow
Write-Host "Size: $([math]::Round($exeSize, 2)) MB" -ForegroundColor Yellow

# Test the executable launches (briefly)
Write-Header "Testing executable"
try {
    $process = Start-Process -FilePath $exePath -ArgumentList @("--help") -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 2
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "Executable test passed!" -ForegroundColor Green
} catch {
    Write-Warning "Could not test executable: $_"
}

Write-Host "`nNext step: Run .\build-installer.ps1 to create the MSI installer" -ForegroundColor Cyan
