# Meta Skill Studio - WPF Edition

A modern, native Windows application built with C# and WPF (Windows Presentation Foundation) for managing AI skills.

## Status: Active Hardening

This WPF application is the supported Windows delivery path for Meta Skill Studio. It currently builds and tests successfully in-repo, but release packaging, documentation, and repository integration are still being hardened.

## Features

- **Modern Windows UI**: Native WPF with Material Design styling
- **MVVM Architecture**: Clean separation of concerns with data binding
- **Async Operations**: Non-blocking UI during skill operations
- **Integrated Python Backend**: Communicates with the existing Python skill engine
- **Single-File Deployment**: Self-contained executable with the .NET runtime embedded
- **Professional Installer**: MSI package for distribution
- **Localization Ready**: Resources.resx for future localization support

## Prerequisites

### Required
- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Python 3.11 or 3.12+** - For the backend skill engine (runtime dependency)

### Optional (for development)
- **Visual Studio 2022** - For XAML designer and debugging
- **WiX Toolset v3.11+** - For building the MSI installer (`winget install WiXToolset.WiXToolset`)

## Quick Start

### 1. Build the Application

Open a terminal in this folder and run:

```powershell
# Build solution
dotnet build MetaSkillStudio.sln

# Run the application
dotnet run --project MetaSkillStudio
```

### 2. Create Single-File Executable

```powershell
dotnet publish MetaSkillStudio -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true
```

Output: `MetaSkillStudio\bin\Release\net8.0-windows\win-x64\publish\MetaSkillStudio.exe`

### 3. Create Installer (Optional)

```powershell
cd installer
.\build-installer.ps1
# Output: MetaSkillStudio-1.0.0.msi
```

## Project Structure

```
windows-wpf/
├── MetaSkillStudio.sln              # Solution file
├── MetaSkillStudio/
│   ├── MetaSkillStudio.csproj       # Project file
│   ├── App.xaml                     # Application resources & styles
│   ├── App.xaml.cs                  # Application startup
│   ├── Resources/
│   │   ├── Resources.resx           # Localization resources
│   │   └── Resources.Designer.cs    # Generated resource accessor
│   ├── Views/
│   │   ├── MainWindow.xaml          # Main window UI (1200x800)
│   │   ├── MainWindow.xaml.cs       # Main window code-behind
│   │   ├── CreateSkillDialog.xaml   # Create skill dialog
│   │   ├── SettingsDialog.xaml      # Runtime configuration
│   │   ├── BenchmarkDialog.xaml     # Benchmark creation
│   │   ├── SkillSelectionDialog.xaml # Skill picker
│   │   ├── PipelineDialog.xaml      # Pipeline execution
│   │   ├── AnalyticsDialog.xaml     # Analytics dashboard
│   │   ├── RunDetailsDialog.xaml    # Run details view
│   │   └── InputDialog.xaml         # Generic input dialog
│   ├── ViewModels/
│   │   ├── MainViewModel.cs         # Main view model
│   │   └── AnalyticsViewModel.cs     # Analytics view model
│   ├── Models/
│   │   └── ApplicationModels.cs      # Data models
│   ├── Services/
│   │   ├── PythonRuntimeService.cs  # Python interop
│   │   ├── ConfigurationStorage.cs   # Settings persistence
│   │   ├── DialogService.cs          # Dialog helpers
│   │   └── Interfaces/               # Service interfaces
│   ├── Converters/
│   │   └── Converters.cs             # XAML value converters
│   └── app.manifest                  # Windows compatibility
├── MetaSkillStudio.Tests/           # Unit tests (xUnit)
└── README.md                        # This file
```

## How It Works

The WPF application acts as a frontend that communicates with the existing Python backend:

```
┌─────────────────────────────────────────────────────────────┐
│                    Meta Skill Studio (WPF)                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  MainWindow  │  │   Commands   │  │   Output Panel   │  │
│  │  (XAML UI)   │  │  (MVVM)      │  │   (Dark theme)   │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────────────┘  │
│         │                 │                                │
│         └─────────────────┘                                │
│                   │                                         │
│         ┌─────────▼─────────┐                              │
│         │ PythonRuntimeService│                             │
│         │   (Process spawn)   │                             │
│         └─────────┬─────────┘                              │
└───────────────────│───────────────────────────────────────────┘
                    │
                    │ stdin/stdout
                    ▼
┌─────────────────────────────────────────────────────────────┐
│              scripts/meta-skill-studio.py                    │
│              (Your existing Python backend)                   │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────┐   │
│  │StudioCore    │ │ meta_skill_  │ │ Runtime calls    │   │
│  │              │ │ studio pkg   │ │ (codex/gemini)   │   │
│  └──────────────┘ └──────────────┘ └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Configuration

### Python Runtime Detection

The app automatically detects Python in this order:
1. Environment variable `PYTHON_PATH`
2. `python` in PATH
3. Common install locations:
   - `C:\Python311\python.exe`
   - `C:\Python312\python.exe`
   - `%LocalAppData%\Microsoft\WindowsApps\python.exe`
   - `%ProgramFiles%\Python311\python.exe`

### Finding the Repository

The app looks for the Meta-Skill-Engineering repository by:
1. Checking `META_SKILL_STUDIO_REPO_ROOT` or `META_SKILL_REPO_ROOT`
2. Looking for `AGENTS.md` from the current working directory upward
3. Falling back to the configured app path and application base directory

## Troubleshooting

### "Python not found" error

Make sure Python 3.11+ is installed and in your PATH:
```powershell
python --version
# Should show Python 3.11.x or higher
```

### "Repository not found" error

Ensure the WPF project is inside or next to your Meta-Skill-Engineering repository, and that `AGENTS.md` exists in the repo root.

### Build errors

Make sure .NET 8.0 SDK is installed:
```powershell
dotnet --version
# Should show 8.0.x or higher
```

### UI scaling issues

The app supports high-DPI displays via `app.manifest` settings. If you see scaling issues:
1. Right-click `MetaSkillStudio.exe` → Properties → Compatibility → Change high DPI settings
2. Check "Override high DPI scaling behavior"
3. Select "Application" from the dropdown

## Customization

### Changing Colors

Edit `App.xaml` - look for the `<Color>` and `<SolidColorBrush>` resources:

```xml
<Color x:Key="PrimaryColor">#2D5AF0</Color>  <!-- Change this -->
```

### Localization

The application is ready for localization via `Resources.resx`. To add a new language:
1. Create `Resources.es.resx` for Spanish, `Resources.fr.resx` for French, etc.
2. Translate the string values
3. Set `Thread.CurrentThread.CurrentUICulture` at startup

## Distribution

### Single Executable

The `dotnet publish` command creates a single-file executable with the .NET runtime embedded:
- Location: `MetaSkillStudio\bin\Release\net8.0-windows\win-x64\publish\MetaSkillStudio.exe`
- Size: ~10-15 MB (includes .NET runtime)
- The Python backend is still required on the target machine for skill execution workflows

### MSI Installer

The WiX installer creates a professional MSI:
- Installs to `C:\Program Files\Meta Skill Studio`
- Creates Start Menu shortcuts
- Creates optional Desktop shortcut
- Registers with Windows Add/Remove Programs
- Supports upgrade/uninstall

## Differences from Python GUI

| Feature | Python tkinter | WPF Edition |
|---------|---------------|-------------|
| Native Look | No | Yes |
| DPI Awareness | Limited | Excellent |
| Async UI | Manual threading | Native async/await |
| Startup Time | Slower | Fast |
| Memory Usage | Higher | Lower |
| Distribution Size | ~20-30 MB | ~10-15 MB |
| Code Changes | Minimal | Full C# rewrite |
| Maintenance | Single codebase | Dual codebase |
| Localization | Hardcoded strings | Resources.resx ready |

## References

- `references/pipeline-definitions.md` - Pipeline specifications
- `references/eval-artifact-schema.md` - Eval artifact formats
- `AGENTS.md` - Repository structure and conventions

## Support

For issues specific to the WPF edition:
1. Check that Python backend works: `python scripts/meta-skill-studio.py --help`
2. Verify .NET SDK: `dotnet --info`
3. Check Windows Event Viewer for crash details
4. Review build output for compilation errors
