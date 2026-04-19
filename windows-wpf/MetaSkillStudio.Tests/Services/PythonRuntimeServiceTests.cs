using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services;
using MetaSkillStudio.Tests.Mocks;
using Xunit;

namespace MetaSkillStudio.Tests.Services
{
    /// <summary>
    /// Unit tests for PythonRuntimeService.
    /// Tests pure functions that don't require actual Python runtime.
    /// </summary>
    public class PythonRuntimeServiceTests
    {
        private readonly PythonRuntimeService _service;

        public PythonRuntimeServiceTests()
        {
            var environmentProvider = new MockEnvironmentProvider();
            var repoRoot = FindRepoRoot();
            environmentProvider.SetCurrentDirectory(repoRoot);
            environmentProvider.SetFileExists(Path.Combine(repoRoot, "AGENTS.md"), true);

            _service = new PythonRuntimeService(
                environmentProvider,
                new MockConfigurationStorage());
        }

        private static string FindRepoRoot()
        {
            var current = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "AGENTS.md")))
                {
                    return current;
                }

                current = Path.GetDirectoryName(current)!;
            }

            throw new DirectoryNotFoundException("Could not find test repository root.");
        }

        #region AddArgumentsToList Tests

        private List<string> InvokeAddArguments(string action, string parameter, TargetLibrary library, int? cases = null)
        {
            var method = typeof(PythonRuntimeService).GetMethod("AddArgumentsToList",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var arguments = new List<string>();
            method.Should().NotBeNull();
            method!.Invoke(_service, new object[] { arguments, action, parameter, library, cases });
            return arguments;
        }

        [Theory]
        [InlineData("create", "Test skill brief", TargetLibrary.LibraryUnverified, null, "LibraryUnverified")]
        [InlineData("create", "Test skill brief", TargetLibrary.LibraryWorkbench, null, "LibraryWorkbench")]
        public void AddArgumentsToList_CreatesCorrectCreateArguments(string action, string parameter, TargetLibrary library, int? cases, string expectedLibrary)
        {
            var arguments = InvokeAddArguments(action, parameter, library, cases);

            arguments.Should().Equal("--mode", "cli", "--action", "create", "--brief", parameter, "--library", expectedLibrary);
        }

        [Theory]
        [InlineData("improve", "skill-name|improvement goal")]
        [InlineData("benchmarks", "skill-name|benchmark goal", 10)]
        public void AddArgumentsToList_HandlesComplexParameters(string action, string complexParameter, int cases = 8)
        {
            var arguments = InvokeAddArguments(action, complexParameter, TargetLibrary.LibraryWorkbench, cases);

            arguments.Should().Contain("--mode");
            arguments.Should().Contain("cli");
            arguments.Should().Contain("--action");
            arguments.Should().Contain(action);
        }

        [Fact]
        public void AddArgumentsToList_KeepsQuotedBriefAsSingleArgument()
        {
            var brief = "text with \"quotes\"";
            var arguments = InvokeAddArguments("create", brief, TargetLibrary.LibraryUnverified);

            arguments.Should().ContainInOrder("--brief", brief);
        }

        [Fact]
        public void AddArgumentsToList_ThrowsOnInvalidAction()
        {
            var method = typeof(PythonRuntimeService).GetMethod("AddArgumentsToList",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var arguments = new List<string>();

            Assert.Throws<TargetInvocationException>(() =>
                method?.Invoke(_service, new object[] { arguments, "invalid-action", "param", TargetLibrary.LibraryUnverified, null }));
        }

        #endregion

        #region ParseJudgeOutput Tests

        [Fact]
        public void ParseJudgeOutput_ExtractsQualityScore()
        {
            // Arrange
            var output = @"
Judge Evaluation Report
Quality Score: 85/100
Some other content
";

            // Act
            var result = _service.ParseJudgeOutput(output);

            // Assert
            result.Should().NotBeNull();
            result!.QualityScore.Should().Be(85);
        }

        [Theory]
        [InlineData("Quality Score: 95/100", 95)]
        [InlineData("quality score: 75/100", 75)]
        [InlineData("Quality Score: 60", 60)]
        [InlineData("quality score 88", 88)]
        public void ParseJudgeOutput_ExtractsVariousScoreFormats(string output, int expectedScore)
        {
            // Act
            var result = _service.ParseJudgeOutput(output);

            // Assert
            result.Should().NotBeNull();
            result!.QualityScore.Should().Be(expectedScore);
        }

        [Fact]
        public void ParseJudgeOutput_ExtractsRoutingNotes()
        {
            // Arrange
            var output = @"
Judge Evaluation Report

Routing Quality Notes:
The skill demonstrated good routing quality with accurate intent recognition.
More content here

Other section:
More text
";

            // Act
            var result = _service.ParseJudgeOutput(output);

            // Assert
            result.Should().NotBeNull();
            result!.RoutingNotes.Should().Contain("routing quality");
        }

        [Fact]
        public void ParseJudgeOutput_ExtractsBehaviorNotes()
        {
            // Arrange
            var output = @"
Judge Evaluation Report

Behavior Quality Notes:
The skill behavior matched expected patterns for the given input.
More content here

Other section:
More text
";

            // Act
            var result = _service.ParseJudgeOutput(output);

            // Assert
            result.Should().NotBeNull();
            result!.BehaviorNotes.Should().Contain("behavior");
        }

        [Fact]
        public void ParseJudgeOutput_ExtractsPriorityFixes()
        {
            // Arrange
            var output = @"
Judge Evaluation Report

Highest Priority Fixes:
- Fix error handling
- Add validation
- Optimize performance

Other section:
More text
";

            // Act
            var result = _service.ParseJudgeOutput(output);

            // Assert
            result.Should().NotBeNull();
            result!.PriorityFixes.Should().NotBeNull();
            result.PriorityFixes.Should().Contain("Fix error handling");
            result.PriorityFixes.Should().Contain("Add validation");
            result.PriorityFixes.Should().Contain("Optimize performance");
        }

        [Fact]
        public void ParseJudgeOutput_ReturnsNullForEmptyInput()
        {
            // Act
            var result = _service.ParseJudgeOutput("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ParseJudgeOutput_ReturnsNullForInvalidInput()
        {
            // Act
            var result = _service.ParseJudgeOutput("Random text without judge output");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region ExtractModelsFromOutput Tests

        [Fact]
        public void ExtractModelsFromOutput_ParsesModelList()
        {
            // Arrange
            var output = @"
gpt-4
gpt-3.5-turbo
claude-3-opus
";

            // Act - using reflection to test private method
            var method = typeof(PythonRuntimeService).GetMethod("ExtractModelsFromOutput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_service, new object[] { output });

            // Assert
            result.Should().BeOfType<List<string>>();
            var models = (List<string>)result!;
            models.Should().Contain("gpt-4");
            models.Should().Contain("gpt-3.5-turbo");
            models.Should().Contain("claude-3-opus");
        }

        [Fact]
        public void ExtractModelsFromOutput_FiltersStopTokens()
        {
            // Arrange
            var output = @"
gpt-4
usage
help
options
gpt-3.5-turbo
";

            // Act
            var method = typeof(PythonRuntimeService).GetMethod("ExtractModelsFromOutput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_service, new object[] { output });

            // Assert
            result.Should().BeOfType<List<string>>();
            var models = (List<string>)result!;
            models.Should().Contain("gpt-4");
            models.Should().Contain("gpt-3.5-turbo");
            models.Should().NotContain("usage");
            models.Should().NotContain("help");
            models.Should().NotContain("options");
        }

        [Fact]
        public void ExtractModelsFromOutput_FiltersNumericOnly()
        {
            // Arrange
            var output = @"
gpt-4
1.0
2.5
3.14
claude-3
";

            // Act
            var method = typeof(PythonRuntimeService).GetMethod("ExtractModelsFromOutput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_service, new object[] { output });

            // Assert
            result.Should().BeOfType<List<string>>();
            var models = (List<string>)result!;
            models.Should().Contain("gpt-4");
            models.Should().Contain("claude-3");
            models.Should().NotContain("1.0");
            models.Should().NotContain("2.5");
            models.Should().NotContain("3.14");
        }

        [Fact]
        public void ExtractModelsFromOutput_LimitsTo30Models()
        {
            // Arrange
            var output = string.Join("\n", Enumerable.Range(1, 50).Select(i => $"model-{i}"));

            // Act
            var method = typeof(PythonRuntimeService).GetMethod("ExtractModelsFromOutput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_service, new object[] { output });

            // Assert
            result.Should().BeOfType<List<string>>();
            var models = (List<string>)result!;
            models.Count.Should().Be(30);
        }

        [Fact]
        public void ExtractModelsFromOutput_SkipsLongLines()
        {
            // Arrange
            var longLine = new string('a', 150);
            var output = $@"
{longLine}
gpt-4
";

            // Act
            var method = typeof(PythonRuntimeService).GetMethod("ExtractModelsFromOutput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_service, new object[] { output });

            // Assert
            result.Should().BeOfType<List<string>>();
            var models = (List<string>)result!;
            models.Should().Contain("gpt-4");
            models.Should().NotContain(longLine);
        }

        #endregion

        #region ListSkills Tests

        [Fact(Skip = "Requires actual file system - integration test")]
        public void ListSkills_ReturnsSkillsFromRepository()
        {
            // This test would require mocking the file system
            // For unit tests, we rely on integration tests for this method
        }

        #endregion

        #region Configuration Tests

        [Fact(Skip = "Requires file system access - integration test")]
        public void LoadConfiguration_ReturnsNullWhenNoConfigExists()
        {
            // Arrange - would need to mock file system
            
            // Act
            var result = _service.LoadConfiguration();

            // Assert
            result.Should().BeNull();
        }

        [Fact(Skip = "Requires file system access - integration test")]
        public void SaveConfiguration_SavesToDisk()
        {
            // This would be an integration test with mocked file system
        }

        #endregion
    }
}
