# MetaSkillStudio Test Infrastructure

## Overview

Comprehensive test suite for the MetaSkillStudio WPF application using xUnit, Moq, and FluentAssertions.

## Project Structure

```
MetaSkillStudio.Tests/
├── MetaSkillStudio.Tests.csproj    # Test project with dependencies
├── Mocks/                            # Mock service implementations
│   ├── MockPythonRuntimeService.cs   # Mock for IPythonRuntimeService
│   ├── MockDialogService.cs          # Mock for IDialogService
│   ├── MockDispatcher.cs             # Mock for IDispatcher
│   ├── MockConfigurationStorage.cs   # Mock for IConfigurationStorage
│   └── MockEnvironmentProvider.cs    # Mock for IEnvironmentProvider
├── Helpers/                          # Test utilities and generators
│   ├── TestDataGenerator.cs          # Factory for test data
│   ├── MockFileSystem.cs             # In-memory file system mock
│   ├── AsyncTestHelper.cs            # Async testing utilities
│   └── AnalyticsCalculatorTests.cs   # Tests for analytics calculations
├── Services/                         # Service unit tests
│   └── PythonRuntimeServiceTests.cs  # Tests for PythonRuntimeService
├── Converters/                       # WPF converter tests
│   └── ConvertersTests.cs            # Tests for value converters
├── ViewModels/                       # ViewModel tests
│   ├── MainViewModelTests.cs         # Tests for MainViewModel
│   └── AnalyticsViewModelTests.cs    # Tests for AnalyticsViewModel
├── Models/                           # Model tests
│   └── ModelTests.cs                 # Tests for model classes
└── Integration/                      # Integration tests
    └── ServiceIntegrationTests.cs    # Tests for service interactions
```

## Test Categories

### 1. Unit Tests

- **ConvertersTests**: Tests for BoolToVisibilityConverter, InverseBoolToVisibilityConverter, StringEmptyToVisibilityConverter
- **PythonRuntimeServiceTests**: Tests for BuildArguments, ParseJudgeOutput, ExtractModelsFromOutput
- **AnalyticsCalculatorTests**: Tests for CalculateTrend and GenerateAlerts
- **ModelTests**: Tests for all model classes (DetectedRuntime, SkillInfo, RunResult, JudgeResult, etc.)
- **MainViewModelTests**: Tests for properties, commands, and service integration
- **AnalyticsViewModelTests**: Tests for data aggregation and alert generation

### 2. Mock Services

- **MockPythonRuntimeService**: Full mock implementation of IPythonRuntimeService with call tracking
- **MockDialogService**: Mock dialog service that tracks all dialog calls and returns configurable results
- **MockDispatcher**: Mock UI dispatcher that executes actions synchronously
- **MockConfigurationStorage**: In-memory configuration storage for testing
- **MockEnvironmentProvider**: Controlled environment variable and path provider

### 3. Test Helpers

- **TestDataGenerator**: Factory methods for creating test data
  - CreateSkillInfo, CreateSkillList
  - CreateRunResult, CreateRunResultList
  - CreateRunInfo, CreateRunInfoList
  - CreateRunMetric, CreateRunMetricList
  - CreateSkillAnalytics, CreateSkillAnalyticsList
  - CreateDetectedRuntime, CreateDetectedRuntimeList
  - CreateJudgeResult, CreateJudgeOutput
  - CreateAppConfiguration

- **MockFileSystem**: In-memory file operations for testing
- **AsyncTestHelper**: Utilities for testing async code

### 4. Integration Tests

- **ServiceIntegrationTests**: Tests for service interactions, DI configuration, and mock behavior

## Dependencies

```xml
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## Running Tests

### Build Tests
```bash
cd windows-wpf
dotnet build MetaSkillStudio.Tests/MetaSkillStudio.Tests.csproj
```

### Run Tests
```bash
cd windows-wpf
dotnet test MetaSkillStudio.Tests/MetaSkillStudio.Tests.csproj
```

### Run with Coverage
```bash
cd windows-wpf
dotnet test MetaSkillStudio.Tests/MetaSkillStudio.Tests.csproj --collect:"XPlat Code Coverage"
```

## Test Features

### Mock Service Features

1. **Call Tracking**: All mocks track method call counts
2. **Parameter Tracking**: ExecuteCommand tracks all parameters passed
3. **Result Queues**: Dialog mocks support multiple sequential results
4. **Failure Simulation**: Configurable exceptions for error testing
5. **Reset Capability**: All mocks support reset for test isolation

### Test Data Features

1. **Random Data**: TestDataGenerator uses Random for varied test data
2. **Trend Data**: Special support for creating trending metrics
3. **Edge Cases**: Helper methods for boundary condition testing

### Async Testing

1. **Synchronous Execution**: AsyncTestHelper.RunSync for testing async methods
2. **Timeout Support**: Built-in timeout handling
3. **Cancellation Testing**: Helpers for testing cancellation tokens

## Coverage Areas

- ✅ Unit tests for all pure functions
- ✅ Mock implementations for all interfaces
- ✅ Test data generators
- ✅ WPF value converter tests
- ✅ Model class tests
- ✅ ViewModel property and command tests
- ✅ Service integration tests
- ✅ Async test helpers

## Notes

The test suite is designed to work with the existing MetaSkillStudio codebase. Some test methods use reflection to test private methods where appropriate. The mocks are designed to be fully configurable and track all interactions for verification.

The main project (MetaSkillStudio.csproj) requires Windows to build and run due to WPF dependencies. The test project is configured with `EnableWindowsTargeting=true` to allow building on non-Windows platforms during CI/CD.
