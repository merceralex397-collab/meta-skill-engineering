# Architecture Guide

**For:** AI Agents and Developers  
**Purpose:** System architecture overview and patterns for MetaSkillStudio  
**Version:** 1.0

---

## System Overview

MetaSkillStudio is a **dual-platform skill engineering environment** with:
- **Python Backend:** CLI tools, automation scripts, and evaluation pipeline
- **WPF Frontend:** Windows desktop application with modern MVVM architecture

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     User Interface Layer                     │
├──────────────────┬──────────────────┬───────────────────────┤
│   WPF Desktop    │   Python CLI     │   Python GUI (Tk)     │
│   (windows-wpf/)│   (scripts/)      │   (meta_skill_studio/)│
└────────┬─────────┴────────┬─────────┴───────────┬───────────┘
         │                    │                     │
┌────────▼────────────────────▼─────────────────────▼─────────┐
│                    Application Services                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │PythonRuntime │  │Configuration │  │Dialog Service    │  │
│  │   Service    │  │   Storage    │  │                  │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
         │
┌────────▼────────────────────────────────────────────────────┐
│                    Python Backend                            │
│         (meta_skill_studio.py + skill scripts)              │
└─────────────────────────────────────────────────────────────┘
         │
┌────────▼────────────────────────────────────────────────────┐
│                    Skill Library                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │  workbench/  │  │LibraryUnveri-│  │ LibraryWorkbench │    │
│  │              │  │    fied/    │  │                  │    │
│  └──────────────┘  └──────────────┘  └──────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## WPF Application Architecture (MVVM)

### MVVM Pattern

**Model-View-ViewModel** separation ensures testability and maintainability:

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│    View     │────▶│  ViewModel   │────▶│     Model       │
│   (XAML)    │◀────│   (C#)       │◀────│    (Data)       │
└─────────────┘     └──────────────┘     └─────────────────┘
      │                                        │
      │         Data Binding                    │
      └────────────────────────────────────────┘
```

### Component Responsibilities

**View (.xaml.cs):**
- Must NOT implement `INotifyPropertyChanged`
- Must NOT contain business logic
- Must delegate to ViewModel for all operations
- Event handlers forward to ViewModel commands

**ViewModel (C#):**
- Implements `INotifyPropertyChanged`
- Receives services via constructor injection
- Contains all business logic
- Exposes `ICommand` properties for UI actions

**Model (C#):**
- Plain data classes with properties
- Uses `[JsonPropertyName]` for serialization
- Inherits from `ObservableModel` base class

### Directory Structure

```
windows-wpf/MetaSkillStudio/
├── Views/           # XAML UI definitions
│   ├── MainWindow.xaml
│   ├── CreateSkillDialog.xaml
│   ├── SettingsDialog.xaml
│   └── ...
├── ViewModels/      # Business logic and data binding
│   ├── MainViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ...
├── Models/          # Data models
│   ├── ApplicationModels.cs
│   └── WorkbenchModels.cs
├── Services/        # Backend integration
│   ├── PythonRuntimeService.cs
│   ├── ConfigurationStorage.cs
│   └── Interfaces/  # Service contracts
├── Commands/        # ICommand implementations
│   └── RelayCommand.cs
├── Converters/      # XAML value converters
├── Helpers/         # Utility classes
└── Extensions/      # Extension methods
```

### Service Layer

**Dependency Injection Pattern:**

```csharp
// Service Interface
public interface IPythonRuntimeService
{
    Task<List<DetectedRuntime>> DetectRuntimesAsync();
    Task<RunResult> ExecuteSkillAsync(string skillName, string action);
}

// Service Implementation
public class PythonRuntimeService : IPythonRuntimeService
{
    public PythonRuntimeService(IEnvironmentProvider envProvider)
    {
        // Constructor injection
    }
}

// ViewModel receives service via DI
public class MainViewModel
{
    public MainViewModel(IPythonRuntimeService pythonService)
    {
        _pythonService = pythonService;
    }
}
```

**DI Registration (App.xaml.cs):**
```csharp
services.AddSingleton<IPythonRuntimeService, PythonRuntimeService>();
services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();
services.AddSingleton<IDialogService, DialogService>();
```

---

## Data Flow

### Typical Operation Flow

1. **User Action** → View captures event
2. **View delegates** → Calls ViewModel command
3. **ViewModel processes** → Business logic execution
4. **Service calls** → Backend communication
5. **Model updates** → Data changes
6. **PropertyChanged** → UI updates via binding

### Example: Creating a Skill

```
User clicks "Create Skill" button
          │
          ▼
CreateSkillDialog View
          │
          ▼
CreateButtonCommand (ICommand in ViewModel)
          │
          ▼
MainViewModel.CreateSkillAsync()
          │
          ▼
PythonRuntimeService.ExecuteSkillAsync("skill-creator", "create")
          │
          ▼
Python backend executes skill-creator
          │
          ▼
Result returned → Model updated
          │
          ▼
PropertyChanged event fired
          │
          ▼
UI updates with new skill list
```

---

## Integration Points

### WPF ↔ Python Communication

**Protocol:** JSON over stdin/stdout via Process

**WPF Side:**
```csharp
var psi = new ProcessStartInfo
{
    FileName = pythonPath,
    Arguments = $"\"{scriptPath}\"",
    UseShellExecute = false,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};

using var process = Process.Start(psi);
process.StandardInput.WriteLine(jsonInput);
string output = process.StandardOutput.ReadToEnd();
var result = JsonSerializer.Deserialize<RunResult>(output);
```

**Security:** Always use `ArgumentList` instead of string concatenation:
```csharp
// CORRECT:
psi.ArgumentList.Add("--skill");
psi.ArgumentList.Add(skillName);

// INCORRECT (CWE-78):
psi.Arguments = $"--skill \"{skillName}\"";
```

### Configuration Storage

**Format:** JSON files in `.meta-skill-studio/`

**Structure:**
```json
{
  "DetectedRuntimes": [...],
  "Roles": {
    "create": { "Runtime": "codex", "Model": "auto" },
    "improve": { "Runtime": "codex", "Model": "auto" }
  }
}
```

**Service:** `ConfigurationStorage` handles read/write with locking

---

## Key Design Decisions

### 1. MVVM Over Code-Behind

**Why:** Testability, separation of concerns, designer/developer workflow

**Rule:** Views contain only UI logic (event forwarding). All business logic in ViewModels.

### 2. Service Interfaces for Testability

**Why:** Mock services for unit testing

**Pattern:** All services implement interfaces, injected via DI

### 3. Async/Await Throughout

**Why:** Keep UI responsive during long operations

**Pattern:**
```csharp
public async Task CreateSkillAsync(string brief)
{
    IsBusy = true;
    try
    {
        var result = await _pythonService.ExecuteSkillAsync(...);
        // Update UI with results
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 4. ObservableModel Base Class

**Why:** Reduce boilerplate for INotifyPropertyChanged

**Pattern:**
```csharp
public class ObservableModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
```

---

## Anti-Patterns to Avoid

### ❌ View Implementing INotifyPropertyChanged

```csharp
// WRONG:
public partial class MyDialog : Window, INotifyPropertyChanged
{
    // View should NOT implement this
}
```

### ❌ Business Logic in Code-Behind

```csharp
// WRONG:
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // File I/O, JSON parsing, validation here
    var json = File.ReadAllText(path);
    var config = JsonSerializer.Deserialize<Config>(json);
    // ...
}
```

### ❌ Direct Service Instantiation

```csharp
// WRONG:
public class MyViewModel
{
    private readonly PythonRuntimeService _service = new PythonRuntimeService();
}
```

### ❌ Synchronous I/O in UI Thread

```csharp
// WRONG:
private void LoadData()
{
    var data = File.ReadAllText(path); // Blocks UI
}
```

---

## Testing Architecture

### Unit Test Structure

```
MetaSkillStudio.Tests/
├── ViewModels/
│   └── MainViewModelTests.cs    # ViewModel logic tests
├── Services/
│   └── PythonRuntimeServiceTests.cs  # Service tests with mocks
├── Converters/
│   └── ConvertersTests.cs       # Value converter tests
└── Mocks/
    ├── MockPythonRuntimeService.cs
    ├── MockDialogService.cs
    └── MockConfigurationStorage.cs
```

### Mock Pattern

```csharp
public class MockPythonRuntimeService : IPythonRuntimeService
{
    public List<DetectedRuntime> DetectedRuntimes { get; set; } = new();
    
    public Task<List<DetectedRuntime>> DetectRuntimesAsync()
    {
        return Task.FromResult(DetectedRuntimes);
    }
}
```

---

## Performance Considerations

### 1. Regex Caching

Use `RegexCache` for compiled regex patterns:
```csharp
var regex = RegexCache.GetOrCreate("pattern", RegexOptions.Compiled);
```

### 2. StringBuilder Reuse

Reuse StringBuilder for large string operations:
```csharp
private readonly StringBuilder _outputBuilder = new();
```

### 3. Async with ConfigureAwait

Library code should use `ConfigureAwait(false)`:
```csharp
var result = await _service.GetDataAsync().ConfigureAwait(false);
```

---

## Related Documentation

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Workflow:** `docs/workflow.md` - Development workflow patterns
- **Code Style:** `docs/code-style.md` - Coding conventions
- **Testing:** `docs/testing-guide.md` - Testing requirements and patterns
- **Security:** `docs/security-guidelines.md` - Security patterns and prevention
- **Troubleshooting:** `docs/troubleshooting.md` - Common issues and solutions
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
