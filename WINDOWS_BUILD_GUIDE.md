# Meta Skill Studio - Windows Build Guide

Meta Skill Studio's supported Windows delivery path is the WPF application in `windows-wpf/`.

The older `windows-build/` Nuitka prototype remains in the repository as historical material only. It is **not** the supported release path and should not be used as a fallback for PR #18 or future Windows releases.

## Supported Build Path

**Location:** `windows-wpf/`

This is the native Windows application built with C# and WPF, backed by the existing Python skill engine.

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 SDK
- Python 3.11 or 3.12+
- WiX Toolset v3.x for MSI packaging
- Visual Studio 2022 (optional, recommended for XAML design/debugging)

## Build and Test

```powershell
cd windows-wpf

dotnet restore MetaSkillStudio.sln
dotnet build MetaSkillStudio.sln -c Release
dotnet test MetaSkillStudio.sln --no-build -c Release
```

## Run the App

```powershell
cd windows-wpf
dotnet run --project MetaSkillStudio
```

## Publish the Portable Executable

```powershell
cd windows-wpf

dotnet publish MetaSkillStudio `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true
```

Expected output:

- `windows-wpf\publish\MetaSkillStudio.exe`

## Build the MSI Installer

```powershell
cd windows-wpf\installer

$publishDir = Join-Path (Resolve-Path ..).Path "publish"
candle.exe -nologo `
  -dProductVersion=1.0.0 `
  -dPublishDir="$publishDir" `
  -out MetaSkillStudio.wixobj `
  MetaSkillStudio.wxs

light.exe -nologo `
  -ext WixUIExtension `
  -out MetaSkillStudio-1.0.0.msi `
  MetaSkillStudio.wixobj
```

## Distribution Notes

- The published executable is self-contained for .NET, but the Python backend is still required for skill execution workflows.
- The MSI installer packages the published WPF executable plus its icon and registration metadata.
- GitHub Actions workflows in `.github/workflows/` are the canonical automation path for CI and release packaging.
