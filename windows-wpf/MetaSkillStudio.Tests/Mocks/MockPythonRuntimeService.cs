using System.Collections.Generic;
using System.Threading.Tasks;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IPythonRuntimeService for testing.
    /// </summary>
    public class MockPythonRuntimeService : IPythonRuntimeService
    {
        private readonly List<DetectedRuntime> _detectedRuntimes = new();
        private readonly List<SkillInfo> _skills = new();
        private readonly List<RunResult> _runResults = new();
        private AppConfiguration? _configuration;

        // Track method calls for verification
        public int DetectRuntimesCallCount { get; private set; }
        public int LoadConfigurationCallCount { get; private set; }
        public int SaveConfigurationCallCount { get; private set; }
        public int CreateDefaultConfigurationCallCount { get; private set; }
        public int ListSkillsCallCount { get; private set; }
        public int ExecuteCommandCallCount { get; private set; }
        public int ParseJudgeOutputCallCount { get; private set; }

        // Track execution parameters
        public List<(string Action, string Parameter, TargetLibrary Library, int? BenchmarkCases)> ExecuteCommandParameters { get; } = new();

        // Configuration for mock behavior
        public bool ShouldThrowOnDetectRuntimes { get; set; }
        public bool ShouldThrowOnExecuteCommand { get; set; }
        public bool ShouldThrowOnSaveConfiguration { get; set; }
        public bool ShouldThrowOnListSkills { get; set; }

        public void AddDetectedRuntime(DetectedRuntime runtime) => _detectedRuntimes.Add(runtime);
        public void AddSkill(SkillInfo skill) => _skills.Add(skill);
        public void AddRunResult(RunResult result) => _runResults.Add(result);
        public void SetConfiguration(AppConfiguration config) => _configuration = config;

        public Task<List<DetectedRuntime>> DetectRuntimesAsync()
        {
            DetectRuntimesCallCount++;
            
            if (ShouldThrowOnDetectRuntimes)
                throw new System.InvalidOperationException("Mock runtime detection failure");

            return Task.FromResult(new List<DetectedRuntime>(_detectedRuntimes));
        }

        public AppConfiguration? LoadConfiguration()
        {
            LoadConfigurationCallCount++;
            return _configuration;
        }

        public void SaveConfiguration(AppConfiguration config)
        {
            SaveConfigurationCallCount++;
            
            if (ShouldThrowOnSaveConfiguration)
                throw new System.InvalidOperationException("Mock save configuration failure");

            _configuration = config;
        }

        public Task<AppConfiguration> CreateDefaultConfigurationAsync()
        {
            CreateDefaultConfigurationCallCount++;
            
            var config = new AppConfiguration
            {
                DetectedRuntimes = new List<DetectedRuntime>(_detectedRuntimes),
                Roles = AppConfiguration.GetDefaultRoles()
            };
            
            _configuration = config;
            return Task.FromResult(config);
        }

        public List<SkillInfo> ListSkills()
        {
            ListSkillsCallCount++;
            if (ShouldThrowOnListSkills)
                throw new System.InvalidOperationException("Mock list skills failure");
            return new List<SkillInfo>(_skills);
        }

        public Task<RunResult> ExecuteCommandAsync(string action, string parameter, TargetLibrary library = TargetLibrary.LibraryUnverified, int? benchmarkCases = null)
        {
            ExecuteCommandCallCount++;
            ExecuteCommandParameters.Add((action, parameter, library, benchmarkCases));

            if (ShouldThrowOnExecuteCommand)
                throw new System.InvalidOperationException("Mock execution failure");

            if (_runResults.Count > 0)
            {
                var queuedResult = _runResults[0];
                _runResults.RemoveAt(0);
                return Task.FromResult(queuedResult);
            }

            // Return a mock result based on the action
            var result = new RunResult
            {
                Action = action,
                ExitCode = 0,
                Stdout = $"Mock output for {action} with parameter: {parameter}",
                Stderr = "",
                DurationSeconds = 1.5,
                StartedAtUtc = System.DateTime.UtcNow,
                EndedAtUtc = System.DateTime.UtcNow,
                Input = new Dictionary<string, object>
                {
                    ["action"] = action,
                    ["parameter"] = parameter
                },
                Artifacts = new Dictionary<string, object>()
            };

            return Task.FromResult(result);
        }

        public JudgeResult? ParseJudgeOutput(string output)
        {
            ParseJudgeOutputCallCount++;

            if (string.IsNullOrEmpty(output))
                return null;

            // Simple mock parsing - look for patterns
            var result = new JudgeResult();

            // Try to find quality score pattern
            var scoreMatch = System.Text.RegularExpressions.Regex.Match(output, @"quality score.*?[:\s]*(\d+)");
            if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var score))
            {
                result.QualityScore = score;
            }

            return result.QualityScore.HasValue ? result : null;
        }

        public void ResetCallCounts()
        {
            DetectRuntimesCallCount = 0;
            LoadConfigurationCallCount = 0;
            SaveConfigurationCallCount = 0;
            CreateDefaultConfigurationCallCount = 0;
            ListSkillsCallCount = 0;
            ExecuteCommandCallCount = 0;
            ParseJudgeOutputCallCount = 0;
            ExecuteCommandParameters.Clear();
        }
    }
}
