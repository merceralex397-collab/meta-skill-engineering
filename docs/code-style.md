# Code Style Guide

**For:** AI Agents and Developers  
**Purpose:** Coding conventions and style standards  
**Version:** 1.0

---

## C# Conventions

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `SkillManager`, `RunResult` |
| Interfaces | PascalCase with I | `IPythonRuntimeService` |
| Methods | PascalCase | `DetectRuntimesAsync` |
| Properties | PascalCase | `IsBusy`, `QualityScore` |
| Private fields | _camelCase | `_pythonService`, `_config` |
| Constants | PascalCase | `MaxTimeoutMinutes` |
| Enums | PascalCase | `WorkbenchState`, `AlertSeverity` |
| Enum values | PascalCase | `Unverified`, `Testing`, `Verified` |

### File Organization

**Order within .cs files:**
1. Using statements (System first, then Microsoft, then project)
2. Namespace declaration
3. Class/interface declaration
4. Constants/static fields
5. Private fields
6. Constructor
7. Public properties
8. Public methods
9. Private methods

### Using Statement Organization

```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 2. Microsoft namespaces
using Microsoft.Extensions.DependencyInjection;

// 3. Project namespaces
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;
```

### Async/Await Patterns

**Always use Async suffix:**
```csharp
// CORRECT:
public async Task<List<Runtime>> DetectRuntimesAsync()

// INCORRECT:
public async Task<List<Runtime>> DetectRuntimes()
```

**ConfigureAwait in library code:**
```csharp
// CORRECT (service layer):
var result = await _service.GetDataAsync().ConfigureAwait(false);

// OK (UI layer):
var result = await _service.GetDataAsync(); // UI context needed
```

**Avoid async void (except for event handlers):**
```csharp
// WRONG:
private async void LoadData() { ... }

// CORRECT:
private async Task LoadDataAsync() { ... }

// OK for events:
private async void Button_Click(object sender, EventArgs e)
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
    }
}
```

### Exception Handling

**Always log exceptions:**
```csharp
// WRONG:
catch (Exception ex)
{
    // Silent failure
}

// CORRECT:
catch (Exception ex)
{
    Debug.WriteLine($"[ComponentName] Operation failed: {ex.Message}");
    // Handle appropriately
}
```

**Required using:**
```csharp
using System.Diagnostics;
```

### Property Patterns

**Observable properties:**
```csharp
private string _name = string.Empty;

public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}
```

**Computed properties:**
```csharp
public string StatusDisplay => _status switch
{
    WorkbenchState.Unverified => "Unverified",
    WorkbenchState.Testing => "Testing",
    WorkbenchState.Verified => "Verified",
    _ => "Unknown"
};
```

### Constructor Patterns

**Dependency injection:**
```csharp
public class SettingsViewModel
{
    private readonly IPythonRuntimeService _pythonService;
    
    public SettingsViewModel(IPythonRuntimeService pythonService)
    {
        _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
    }
}
```

### XML Documentation

**Required for public APIs:**
```csharp
/// <summary>
/// Detects available Python runtimes on the system.
/// </summary>
/// <param name="cancellationToken">Cancellation token for the operation.</param>
/// <returns>List of detected runtimes.</returns>
public async Task<List<DetectedRuntime>> DetectRuntimesAsync(CancellationToken cancellationToken = default)
```

---

## XAML Conventions

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| XAML files | PascalCase + Dialog/Window | `CreateSkillDialog.xaml` |
| x:Name | camelCase | `skillListBox`, `createButton` |
| Styles | PascalCase + Style | `PrimaryButtonStyle` |
| Resources | PascalCase | `PrimaryBrush`, `BorderBrush` |

### Control Structure

```xml
<!-- Order of attributes -->
<Button 
    x:Name="createButton"
    Grid.Row="1"
    Grid.Column="0"
    Style="{StaticResource PrimaryButton}"
    Content="Create Skill"
    ToolTip="Create a new skill"
    Click="CreateButton_Click"
    AutomationProperties.Name="Create Skill"
    AutomationProperties.HelpText="Click to create a new skill" />
```

### Accessibility Requirements

Every interactive element must have:
- `ToolTip` for hover help
- `AutomationProperties.Name` for screen readers
- `TabIndex` for keyboard navigation (where logical order differs from visual)

### Grid Layout

```xml
<!-- Unique Grid.Row for each element -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>
    
    <!-- Row 0 -->
    <TextBlock Grid.Row="0" Text="Header" />
    
    <!-- Row 1 -->
    <ListBox Grid.Row="1" ... />
    
    <!-- Row 2 -->
    <StackPanel Grid.Row="2" ... />
</Grid>
```

**Rule:** Never have two elements with the same `Grid.Row` value.

### Data Binding

```xml
<!-- Two-way for editable data -->
<TextBox Text="{Binding SkillName, Mode=TwoWay}" />

<!-- One-way for read-only display -->
<TextBlock Text="{Binding StatusText}" />

<!-- With converters -->
<ProgressBar Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}" />
```

### Validation

```xml
<!-- Only on the validating control -->
<TextBox Validation.ErrorTemplate="{StaticResource ErrorTemplate}">
    <TextBox.Text>
        <Binding Path="BriefText" Mode="TwoWay">
            <Binding.ValidationRules>
                <validators:NotEmptyValidationRule />
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

**Rule:** Never put `Validation.ErrorTemplate` on container elements (Border, Panel).

---

## SKILL.md Conventions

### Structure (Strict Order)

1. YAML frontmatter
2. Purpose
3. When to use
4. When NOT to use
5. Procedure
6. Output contract
7. Failure handling
8. Next steps
9. References (optional)

### YAML Frontmatter

```yaml
---
name: skill-name
description: >-
  [Action verb] [specific object] when [task conditions].
  Second sentence with additional context.
---
```

**Description rules:**
- Action verb first
- 2-3 realistic trigger phrases
- State what skill produces
- "Do not use for..." with named alternatives
- ~100 words maximum

### Section Headings

**Use exact heading text:**
```markdown
## Purpose
## When to use
## When NOT to use
## Procedure
## Output contract
## Failure handling
## Next steps
```

**Do NOT use:**
```markdown
## Do NOT use when:  ← WRONG heading text
## Output Contract  ← Wrong capitalization
## failure handling ← Wrong capitalization
```

### Size Limits

- **SKILL.md:** < 500 lines
- **Description:** ~100 words
- **Critical steps:** In first 40% of file
- **References:** Extract if > 300 lines

### Procedure Steps

**Format:**
```markdown
## Procedure

### Phase 1: Capture Intent
1. Read the task description completely
2. Identify required outputs
3. Note any constraints or boundaries

### Phase 2: Execute
1. [Concrete action verb] [specific target]
2. [Concrete action verb] [specific target]
```

**Rules:**
- Every step starts with verb: Read, List, Write, Check, Run, Compare
- No hedge verbs: "Consider", "Think about"
- No meta-commentary: "Keep scope explicit" → "List all file paths"

---

## Python Conventions

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| Modules | snake_case | `meta_skill_studio.py` |
| Classes | PascalCase | `StudioCore`, `SkillRunner` |
| Functions | snake_case | `detect_runtimes()`, `execute_skill()` |
| Constants | UPPER_SNAKE_CASE | `MAX_TIMEOUT`, `DEFAULT_MODEL` |
| Private | _leading_underscore | `_internal_helper()` |

### Type Hints

```python
from __future__ import annotations
from typing import List, Dict, Optional

def detect_runtimes(python_path: str) -> List[DetectedRuntime]:
    ...

def execute_skill(
    skill_name: str,
    action: str,
    timeout: Optional[int] = None
) -> RunResult:
    ...
```

### Docstrings

```python
def detect_runtimes(python_path: str) -> List[DetectedRuntime]:
    """
    Detect available Python runtimes on the system.
    
    Args:
        python_path: Path to Python executable to check
        
    Returns:
        List of detected runtimes with metadata
        
    Raises:
        FileNotFoundError: If python_path doesn't exist
    """
```

### Error Handling

```python
# Always catch specific exceptions
try:
    result = subprocess.run(cmd, capture_output=True, text=True)
except subprocess.TimeoutExpired as e:
    logger.error(f"Command timed out after {timeout}s: {cmd}")
    raise RuntimeError(f"Timeout: {e}") from e
except subprocess.CalledProcessError as e:
    logger.error(f"Command failed with exit code {e.returncode}")
    raise RuntimeError(f"Command failed: {e.stderr}") from e
```

---

## JSON/JSONL Conventions

### trigger-positive.jsonl

```json
{"prompt": "Create a skill for parsing JSON files", "expected": "trigger", "category": "core", "notes": "Direct request for skill creation"}
{"prompt": "I need to add error handling to my skill", "expected": "trigger", "category": "indirect", "notes": "Implies existing skill improvement"}
```

### behavior.jsonl

```json
{
  "prompt": "Create a skill for parsing JSON files",
  "expected_sections": ["Purpose", "When to use", "Procedure"],
  "required_patterns": ["JSON", "parse", "file"],
  "forbidden_patterns": ["TODO", "FIXME"],
  "min_output_lines": 15,
  "notes": "Should produce complete skill structure"
}
```

---

## Git Commit Conventions

### Commit Message Format

```
[<scope>] <action>: <description>

- Finding F-001: Fixed ambiguous type references
- Finding F-002: Added using aliases

Verification: Build passes (0 errors)
```

### Small Commits Rule

- Maximum 3 findings per commit
- Each commit must reference finding IDs
- Build must pass before committing

---

## Related Documentation

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Workflow:** `docs/workflow.md` - Development workflow patterns
- **Architecture:** `docs/architecture.md` - System architecture and patterns
- **Testing:** `docs/testing-guide.md` - Testing requirements and patterns
- **Security:** `docs/security-guidelines.md` - Security patterns and prevention
- **Troubleshooting:** `docs/troubleshooting.md` - Common issues and solutions
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
