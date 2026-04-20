# Troubleshooting Guide

**For:** AI Agents and Developers  
**Purpose:** Common issues and solutions  
**Version:** 1.0

---

## Build Errors

### CS0104: Ambiguous Reference

**Error:**
```
'Brush' is an ambiguous reference between 'System.Windows.Media.Brush' 
and 'System.Drawing.Brush'
```

**Cause:** Project has both `UseWPF` and `UseWindowsForms` enabled.

**Solution:**
Add explicit using alias at top of file:
```csharp
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;
```

---

### CS0246: Type Not Found

**Error:**
```
The type or namespace name 'RelayCommand' could not be found
```

**Cause:** Missing using statement after moving shared component.

**Solution:**
Add to consuming files:
```csharp
using MetaSkillStudio.Commands;
```

---

### CS1061: Missing Property

**Error:**
```
'RunResult' does not contain a definition for 'StartedAt'
```

**Cause:** Property renamed to `StartedAtUtc` but references not updated.

**Solution:**
Search/replace in all files:
- `StartedAt` → `StartedAtUtc`
- `EndedAt` → `EndedAtUtc`
- `LastUpdated` → `LastUpdatedUtc`

---

### CS1519: Invalid Token

**Error:**
```
Invalid token 'return' in class, record, struct, or interface member declaration
```

**Cause:** Duplicate code lines from edit operation.

**Solution:**
1. Read the file
2. Find and remove duplicate lines
3. Verify structure is valid

**Prevention:** Always read modified section after editing.

---

## XAML Errors

### Duplicate Grid.Row

**Error:** Two elements with same `Grid.Row` value causing layout issues.

**Solution:** Assign unique row values:
```xml
<!-- Before -->
<CheckBox Grid.Row="3" ... />
<StackPanel Grid.Row="3" ... />  <!-- Same row! -->

<!-- After -->
<CheckBox Grid.Row="2" ... />    <!-- Fixed -->
<StackPanel Grid.Row="3" ... />
```

---

### Duplicate Validation.ErrorTemplate

**Error:** `Validation.ErrorTemplate` on both Border and TextBox.

**Solution:** Remove from container, keep only on validating control:
```xml
<!-- Before -->
<Border Validation.ErrorTemplate="{StaticResource ErrorTemplate}">
    <TextBox Validation.ErrorTemplate="{StaticResource ErrorTemplate}" />
</Border>

<!-- After -->
<Border>  <!-- No ErrorTemplate -->
    <TextBox Validation.ErrorTemplate="{StaticResource ErrorTemplate}" />
</Border>
```

---

### Binding Failures

**Error:** Data not appearing in UI despite binding.

**Check:**
1. Property implements `INotifyPropertyChanged`
2. Property name matches binding exactly (case-sensitive)
3. DataContext is set correctly
4. No binding errors in Output window

**Debug:**
```csharp
// Add to ViewModel constructor
PropertyChanged += (s, e) => 
    Debug.WriteLine($"PropertyChanged: {e.PropertyName}");
```

---

## Runtime Issues

### Python Not Found

**Error:** `Python runtime not detected`

**Solutions:**
1. Install Python 3.11+ from python.org
2. Or install from Microsoft Store
3. Ensure Python is in PATH
4. Restart application

**Manual configuration:**
Edit `.meta-skill-studio/config.json`:
```json
{
  "DetectedRuntimes": [
    {
      "Name": "manual",
      "Path": "C:\\Python311\\python.exe",
      "IsAvailable": true
    }
  ]
}
```

---

### Process Timeout

**Error:** `Operation timed out after 30 minutes`

**Cause:** Long-running skill execution exceeded timeout.

**Solutions:**
1. Check Python backend is responding
2. Reduce skill complexity
3. Increase timeout in Settings (not recommended)
4. Check for infinite loops in skill

---

### LSP Errors in VS Code

**Error:** Red squiggles showing missing references.

**Cause:** Multi-targeting or missing restore.

**Solutions:**
```bash
# Restore packages
dotnet restore

# Rebuild
dotnet build --no-incremental

# Clear obj/bin and rebuild
rm -rf windows-wpf/MetaSkillStudio/obj
rm -rf windows-wpf/MetaSkillStudio/bin
dotnet build
```

---

## Git Issues

### Branch Divergence

**Error:** `Your branch and 'origin/main' have diverged`

**Solution:**
```bash
# See what commits differ
git log --oneline --left-right HEAD...origin/main

# Option 1: Merge
git fetch origin
git merge origin/main

# Option 2: Rebase (if no shared work)
git fetch origin
git rebase origin/main

# Option 3: Fresh clone (nuclear option)
# Backup work first!
```

---

### Large Session Files

**Issue:** `session-full.json` is 80MB and slowing operations.

**Solution:**
```bash
# Remove transient build artifacts from session file
# Or exclude from git tracking
echo "session-full.json" >> .gitignore
```

---

## Testing Issues

### Tests Pass Individually but Fail Together

**Cause:** Shared state between tests.

**Solution:**
```csharp
// Use collection isolation
[Collection("Non-Parallel")]
public class MyTests
{
    // Tests won't run in parallel
}
```

---

### Mock Not Called

**Error:** `Expected mock to be called 1 time, but was called 0 times`

**Check:**
1. Mock was passed to constructor
2. Correct method name (case-sensitive)
3. Correct parameters (use `It.IsAny<T>()` if flexible)
4. Async method awaited in test

---

## SKILL.md Issues

### Skill Not Triggering

**Cause:** Description doesn't match user query.

**Solution:**
Add more trigger phrases to description:
```yaml
description: >-
  Create a new skill from a brief description.
  Use when: "create a skill for X", "I need a skill that does Y",
  "build a skill to handle Z"
```

---

### Eval Tests Failing

**Cause:** Behavior doesn't match expected patterns.

**Debug:**
```bash
# Run specific test
./scripts/run-evals.sh skill-name

# Dry run to see test cases
./scripts/run-evals.sh --dry-run skill-name

# Check output
./scripts/run-evals.sh --dry-run skill-name 2>&1 | head -50
```

---

## Performance Issues

### Slow Application Startup

**Check:**
1. Python runtime detection taking long
2. Large workbench/ directory
3. Debug builds (use Release)

**Solutions:**
```bash
# Build Release configuration
dotnet build -c Release

# Skip runtime detection in Settings
# (use manual configuration)
```

---

### UI Freezing During Operations

**Cause:** Synchronous I/O on UI thread.

**Solution:** Use async/await:
```csharp
// Wrong
private void LoadData()
{
    var data = File.ReadAllText(path); // Blocks UI
}

// Correct
private async Task LoadDataAsync()
{
    var data = await File.ReadAllTextAsync(path); // Non-blocking
}
```

---

## Getting Help

### Debug Logging

Enable verbose logging:
```csharp
// In ViewModel or Service
Debug.WriteLine($"[Component] Operation started: {DateTime.Now}");
```

View in Output window (VS Code: Debug Console).

### Diagnostic Build

```bash
# Detailed build output
dotnet build -v detailed 2>&1 | tee build.log

# Look for warnings (treated as errors in strict mode)
grep -i warning build.log
```

### Session Analysis

If issues persist, review session logs:
```bash
# Latest session
cat plans/docplans/log1.md | tail -100

# Search for errors
grep -n "error\|fail\|exception" plans/docplans/log1.md
```

---

## Quick Reference Card

| Issue | Quick Fix |
|-------|-----------|
| CS0104 | Add using aliases |
| CS0246 | Add missing using |
| CS1061 | Update property names (Utc suffix) |
| CS1519 | Remove duplicate lines |
| Build fails | `dotnet restore && dotnet build` |
| Python not found | Install Python 3.11+, restart |
| UI frozen | Make method async, use await |
| Test fails | Check mock setup, use async/await |
| Skill won't trigger | Add more trigger phrases |

---

## Related Documentation

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Workflow:** `docs/workflow.md` - Development workflow patterns
- **Architecture:** `docs/architecture.md` - System architecture and patterns
- **Code Style:** `docs/code-style.md` - Coding conventions
- **Testing:** `docs/testing-guide.md` - Testing requirements and patterns
- **Security:** `docs/security-guidelines.md` - Security patterns and prevention
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
