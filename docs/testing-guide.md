# Testing Guide

**For:** AI Agents and Developers  
**Purpose:** Testing requirements, patterns, and procedures  
**Version:** 1.0

---

## Testing Philosophy

**Primary Rule:** Tests prove correctness, not just existence.

- Counting test files is NOT verification
- Listing test methods is NOT validation
- Tests must exercise actual behavior and verify outcomes

---

## Test Organization

### Directory Structure

```
MetaSkillStudio.Tests/
├── ViewModels/              # ViewModel logic tests
│   ├── MainViewModelTests.cs
│   └── AnalyticsViewModelTests.cs
├── Services/                # Service implementation tests
│   └── PythonRuntimeServiceTests.cs
├── Converters/              # Value converter tests
│   └── ConvertersTests.cs
├── Models/                  # Data model tests
│   └── ModelTests.cs
├── Helpers/                 # Utility class tests
│   ├── TestDataGenerator.cs
│   ├── AsyncTestHelper.cs
│   └── AnalyticsCalculatorTests.cs
├── Mocks/                   # Mock service implementations
│   ├── MockPythonRuntimeService.cs
│   ├── MockDialogService.cs
│   ├── MockConfigurationStorage.cs
│   └── MockEnvironmentProvider.cs
└── Integration/             # Integration tests
    └── ServiceIntegrationTests.cs
```

### Test File Naming

- Test class: `{ClassName}Tests.cs`
- Method: `{MethodName}_{Scenario}_{ExpectedResult}`

Example: `CreateSkillAsync_ValidBrief_ReturnsSuccessResult`

---

## Unit Testing Patterns

### Arrange-Act-Assert Structure

```csharp
[Fact]
public async Task ExecuteSkillAsync_ValidSkill_ReturnsSuccess()
{
    // Arrange
    var mockPythonService = new MockPythonRuntimeService
    {
        DetectedRuntimes = new List<DetectedRuntime>
        {
            new() { Name = "codex", IsAvailable = true }
        }
    };
    var viewModel = new MainViewModel(mockPythonService);
    
    // Act
    var result = await viewModel.ExecuteSkillAsync("skill-creator", "create");
    
    // Assert
    result.Should().BeTrue();
    mockPythonService.ExecuteCallCount.Should().Be(1);
}
```

### Mock Service Pattern

```csharp
public class MockPythonRuntimeService : IPythonRuntimeService
{
    public List<DetectedRuntime> DetectedRuntimes { get; set; } = new();
    public int DetectCallCount { get; set; }
    public int ExecuteCallCount { get; set; }
    
    public Task<List<DetectedRuntime>> DetectRuntimesAsync()
    {
        DetectCallCount++;
        return Task.FromResult(DetectedRuntimes);
    }
    
    public Task<RunResult> ExecuteSkillAsync(string skill, string action)
    {
        ExecuteCallCount++;
        return Task.FromResult(new RunResult { Success = true });
    }
}
```

### INotifyPropertyChanged Testing

```csharp
[Fact]
public void IsBusy_SetToTrue_RaisesPropertyChanged()
{
    // Arrange
    var viewModel = new MainViewModel(_mockService);
    var propertyChangedRaised = false;
    viewModel.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(viewModel.IsBusy))
            propertyChangedRaised = true;
    };
    
    // Act
    viewModel.IsBusy = true;
    
    // Assert
    propertyChangedRaised.Should().BeTrue();
}
```

### Async Test Patterns

```csharp
[Fact]
public async Task LoadDataAsync_Cancellation_ThrowsOperationCancelled()
{
    // Arrange
    var cts = new CancellationTokenSource();
    cts.Cancel();
    
    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _service.LoadDataAsync(cts.Token));
}
```

---

## Test Data Management

### TestDataGenerator

Central factory for creating test data:

```csharp
public static class TestDataGenerator
{
    public static RunResult CreateSuccessResult(string skillName = "test-skill")
    {
        return new RunResult
        {
            Success = true,
            SkillName = skillName,
            StartedAtUtc = DateTime.UtcNow,
            EndedAtUtc = DateTime.UtcNow.AddSeconds(5)
        };
    }
    
    public static RunResult CreateFailureResult(string error)
    {
        return new RunResult
        {
            Success = false,
            ErrorOutput = error
        };
    }
    
    public static List<RunResult> CreateRunHistory(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateSuccessResult($"skill-{i}"))
            .ToList();
    }
}
```

### Async Test Helper

```csharp
public static class AsyncTestHelper
{
    public static void RunSync(Func<Task> action)
    {
        // Use only in tests, not production
        Task.Run(action).Wait();
    }
    
    public static T RunSync<T>(Func<Task<T>> action)
    {
        return Task.Run(action).Result;
    }
}
```

---

## Test Coverage Requirements

### Minimum Coverage by Component

| Component | Minimum Coverage | Priority |
|-----------|-----------------|----------|
| ViewModels | 80% | High |
| Services | 70% | High |
| Models | 60% | Medium |
| Converters | 90% | High |
| Helpers | 70% | Medium |

### Critical Paths (Must Test)

1. **PythonRuntimeService**
   - Runtime detection
   - Skill execution (success and failure)
   - Timeout handling
   - Cancellation

2. **MainViewModel**
   - Property change notifications
   - Command execution
   - Error handling
   - Busy state management

3. **ConfigurationStorage**
   - Load/save configuration
   - Corrupted file handling
   - Concurrent access

4. **Converters**
   - All converter implementations
   - Edge cases (null, empty, invalid input)

---

## Testing Procedures

### Running Tests

```bash
# All tests
dotnet test MetaSkillStudio.Tests/

# Specific test class
dotnet test --filter "FullyQualifiedName~MainViewModelTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Verbose output
dotnet test --verbosity normal
```

### Pre-Commit Testing

Before committing changes:

1. **Run affected tests:**
   ```bash
   dotnet test --filter "FullyQualifiedName~{ModifiedClassName}"
   ```

2. **Verify no regressions:**
   ```bash
   dotnet test
   ```

3. **Check for test gaps:**
   - Did you add tests for new functionality?
   - Did you update tests for modified behavior?

### Verification Checklist

Before claiming tests complete:

- [ ] New functionality has corresponding tests
- [ ] Modified behavior has updated tests
- [ ] Edge cases are covered
- [ ] Async methods tested with async/await (not .Result/.Wait)
- [ ] INotifyPropertyChanged events verified
- [ ] Mock services track call counts and parameters
- [ ] All tests pass (dotnet test returns 0)
- [ ] No test warnings or skipped tests

---

## Integration Testing

### Service Integration

```csharp
[Fact]
public async Task FullPipeline_CreateSkill_ExecutesAndSaves()
{
    // Arrange - Real services with temp storage
    var tempConfig = Path.GetTempFileName();
    var configStorage = new ConfigurationStorage(
        new MockEnvironmentProvider(tempConfig));
    var pythonService = new PythonRuntimeService(
        new MockEnvironmentProvider(), configStorage);
    
    try
    {
        // Act
        var result = await pythonService.ExecuteSkillAsync(
            "skill-creator", "create", "test brief");
        
        // Assert
        result.Success.Should().BeTrue();
        var config = await configStorage.LoadAsync();
        config.Roles.Should().ContainKey("create");
    }
    finally
    {
        File.Delete(tempConfig);
    }
}
```

### UI Integration (Manual)

Test scenarios requiring manual verification:

1. **Dialog workflows**
   - Create skill flow
   - Settings configuration
   - Run result viewing

2. **Error presentation**
   - Python not found error
   - Skill execution failure
   - Network timeout

3. **Accessibility**
   - Keyboard navigation
   - Screen reader compatibility
   - High contrast mode

---

## Common Testing Pitfalls

### ❌ Testing Implementation Details

```csharp
// WRONG: Testing private method behavior
[Fact]
public void InternalHelper_DoesSomething()
{
    // Don't test private implementation
}

// CORRECT: Testing public behavior
[Fact]
public void ExecuteSkillAsync_Success_UpdatesSkillList()
{
    // Test public API behavior
}
```

### ❌ Flaky Async Tests

```csharp
// WRONG: Non-deterministic timing
[Fact]
public async Task LoadData_TimesOut()
{
    await Task.Delay(100); // Flaky
    // ...
}

// CORRECT: Controlled cancellation
[Fact]
public async Task LoadData_Cancellation_Throws()
{
    var cts = new CancellationTokenSource();
    cts.Cancel(); // Deterministic
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _service.LoadAsync(cts.Token));
}
```

### ❌ Counting as Verification

```csharp
// WRONG: Just counting tests
[Fact]
public void HasTests()
{
    var testCount = typeof(MainViewModelTests).GetMethods()
        .Count(m => m.GetCustomAttribute<FactAttribute>() != null);
    testCount.Should().BeGreaterThan(0); // Meaningless
}

// CORRECT: Testing actual behavior
[Fact]
public void ExecuteCommand_CanExecuteFalse_DoesNotCallService()
{
    // Verify actual behavior
}
```

---

## Evaluation Testing (Skill-Level)

### Eval File Testing

Every skill must have eval files tested:

```bash
# Run all evals for a skill
./scripts/run-evals.sh skill-name

# Dry run (show test cases without executing)
./scripts/run-evals.sh --dry-run skill-name

# Run evals for all skills
./scripts/run-evals.sh --all
```

### Minimum Eval Requirements

| Eval Type | Minimum Cases |
|-----------|--------------|
| trigger-positive.jsonl | 3 cases |
| trigger-negative.jsonl | 3 cases |
| behavior.jsonl | 2 cases |

### Eval Verification Checklist

- [ ] All eval files parse as valid JSONL
- [ ] trigger-positive cases actually trigger the skill
- [ ] trigger-negative cases do NOT trigger the skill
- [ ] behavior cases produce expected output structure
- [ ] No eval failures in strict mode

---

## Related Documentation

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Workflow:** `docs/workflow.md` - Development workflow patterns
- **Architecture:** `docs/architecture.md` - System architecture and patterns
- **Code Style:** `docs/code-style.md` - Coding conventions
- **Security:** `docs/security-guidelines.md` - Security patterns and prevention
- **Troubleshooting:** `docs/troubleshooting.md` - Common issues and solutions
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
