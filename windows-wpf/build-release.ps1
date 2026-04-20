#Requires -Version 5.1
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "publish"
)

$ErrorActionPreference = "Stop"

$workspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $PSScriptRoot "MetaSkillStudio\MetaSkillStudio.csproj"
$publishOutput = Join-Path $PSScriptRoot "MetaSkillStudio\bin\$Configuration\net8.0-windows\$Runtime\publish"
$stagingDir = Join-Path $PSScriptRoot $OutputDir
$opencodeNodeModules = Join-Path $workspaceRoot ".opencode\node_modules"

function Copy-WorkspaceItem {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $source = Join-Path $workspaceRoot $RelativePath
    if (-not (Test-Path $source)) {
        Write-Warning "Skipping missing workspace item: $RelativePath"
        return
    }

    $destination = Join-Path $stagingDir $RelativePath
    $destinationParent = Split-Path $destination -Parent
    if (-not [string]::IsNullOrWhiteSpace($destinationParent)) {
        New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null
    }

    Copy-Item $source $destination -Recurse -Force
}

if (-not (Test-Path $opencodeNodeModules)) {
    Write-Host "Installing repo-local OpenCode SDK/runtime dependencies..."
    npm install --prefix (Join-Path $workspaceRoot ".opencode")
}

Write-Host "Publishing Meta Skill Studio..."
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true

$stagedExe = Join-Path $stagingDir "MetaSkillStudio.exe"
$runningBundleProcesses = Get-Process -ErrorAction SilentlyContinue | Where-Object {
    try {
        $_.Path -eq $stagedExe
    }
    catch {
        $false
    }
}

if ($runningBundleProcesses) {
    $processIds = $runningBundleProcesses | Select-Object -ExpandProperty Id
    throw "Close Meta Skill Studio running from $stagingDir before rebuilding the release bundle. PID(s): $($processIds -join ', ')"
}

if (Test-Path $stagingDir) {
    Remove-Item $stagingDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null
Copy-Item (Join-Path $publishOutput "*") $stagingDir -Recurse -Force

$workspaceItems = @(
    "AGENTS.md",
    "README.md",
    "windows-wpf\README.md",
    "docs",
    "scripts",
    ".opencode",
    "LibraryUnverified",
    "LibraryWorkbench"
)

$skillPackageDirs = Get-ChildItem $workspaceRoot -Directory |
    Where-Object { Test-Path (Join-Path $_.FullName "SKILL.md") } |
    Sort-Object Name |
    Select-Object -ExpandProperty Name

foreach ($relativePath in ($workspaceItems + $skillPackageDirs | Select-Object -Unique)) {
    Copy-WorkspaceItem -RelativePath $relativePath
}

$requiredPaths = @(
    "MetaSkillStudio.exe",
    "AGENTS.md",
    "scripts\meta-skill-studio.py",
    ".opencode\opencode.json",
    "LibraryUnverified"
)

foreach ($requiredPath in $requiredPaths) {
    $stagedPath = Join-Path $stagingDir $requiredPath
    if (-not (Test-Path $stagedPath)) {
        throw "Release bundle is missing required path: $requiredPath"
    }
}

Write-Host "Release bundle ready at $stagingDir"
