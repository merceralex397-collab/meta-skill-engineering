# Meta Skill Studio - Archived Windows Build Prototype

## Overview

This directory contains an older Nuitka-based Windows packaging prototype.

It is retained for reference only. The supported Windows release path for this repository is the WPF application in `windows-wpf/`.

## Status

- Historical prototype
- Not the supported release path
- Not a fallback for PR #18

## Legacy Build Requirements

### On Windows (Recommended)
- Python 3.11 or 3.12
- Visual Studio 2022 Build Tools (or full VS2022)
- WiX Toolset v3.11 or v4.x
- Git for Windows

### On Linux (Cross-compilation not supported for GUI apps)
You must build on Windows for Windows GUI applications.

## Legacy Quick Start

### 1. Install Build Dependencies

```powershell
# Install Nuitka (legacy prototype only)
pip install nuitka zstandard

# Install WiX Toolset (using winget)
winget install WiXToolset.WiXToolset

# Or download from: https://wixtoolset.org/
```

### 2. Build the Executable

```powershell
cd windows-build
.\build.ps1
```

This creates a legacy experimental artifact:
- `dist\MetaSkillStudio.exe`

### 3. Build the Installer

```powershell
.\build-installer.ps1
```

This creates a legacy experimental installer:
- `dist\MetaSkillStudio-1.0.0.msi`

## Build Outputs

| File | Description |
|------|-------------|
| `MetaSkillStudio.exe` | Legacy prototype executable |
| `MetaSkillStudio-1.0.0.msi` | Legacy prototype installer |

## Legacy Characteristics

- **Nuitka-based compilation**
- **Single-file executable output**
- **Prototype MSI installer scripts**
- **Not the supported shipping path**

## Technical Details

### Nuitka Compilation
- Uses Nuitka's onefile mode for single executable
- Embeds Python interpreter and all dependencies
- Includes tkinter for the GUI
- Optimized for size and startup time

### Installer Features
- Installs to `Program Files\Meta Skill Studio`
- Creates Start Menu shortcuts
- Creates Desktop shortcut (optional)
- Registers uninstaller in Windows
- Creates file associations (optional)
- Supports per-user and per-machine installation

## Development

### Testing the Executable

```powershell
# Run without installing
dist\MetaSkillStudio.exe

# With console output for debugging
dist\MetaSkillStudio.exe --console
```

### Debugging Build Issues

```powershell
# Verbose build output
python -m nuitka --verbose src\MetaSkillStudio.py

# Show memory usage
python -m nuitka --show-memory src\MetaSkillStudio.py
```

## Distribution

Do not use this path for current releases. Use `windows-wpf/` and the WPF-specific build guidance instead.
