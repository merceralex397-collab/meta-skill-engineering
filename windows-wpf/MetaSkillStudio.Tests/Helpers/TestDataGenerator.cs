using System;
using System.Collections.Generic;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.Tests.Helpers
{
    /// <summary>
    /// Generates test data for unit tests.
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random Random = new();

        #region SkillInfo

        public static SkillInfo CreateSkillInfo(string? name = null, string? description = null, bool hasSkillMd = true, bool hasEvals = true)
        {
            var skillName = name ?? $"skill-{Guid.NewGuid():N}";
            return new SkillInfo
            {
                Name = skillName,
                DirectoryPath = $"/home/rowan/Meta-Skill-Engineering/{skillName}",
                Description = description ?? $"Test skill description for {skillName}",
                HasSkillMd = hasSkillMd,
                HasEvals = hasEvals
            };
        }

        public static List<SkillInfo> CreateSkillList(int count = 5)
        {
            var skills = new List<SkillInfo>();
            for (int i = 0; i < count; i++)
            {
                skills.Add(CreateSkillInfo(name: $"test-skill-{i}"));
            }
            return skills;
        }

        #endregion

        #region RunResult

        public static RunResult CreateRunResult(
            string action = "test",
            string parameter = "",
            int exitCode = 0,
            string? stdout = null,
            string? stderr = null,
            double durationSeconds = 5.0)
        {
            var startTime = DateTime.UtcNow.AddMinutes(-Random.Next(1, 100));
            
            return new RunResult
            {
                Action = action,
                ExitCode = exitCode,
                Stdout = stdout ?? $"Mock stdout for {action}",
                Stderr = stderr ?? "",
                DurationSeconds = durationSeconds,
                StartedAtUtc = startTime,
                EndedAtUtc = startTime.AddSeconds(durationSeconds),
                Input = new Dictionary<string, object>
                {
                    ["action"] = action,
                    ["parameter"] = parameter
                },
                Artifacts = new Dictionary<string, object>()
            };
        }

        public static RunResult CreateFailedRunResult(string action = "test", string parameter = "", string errorMessage = "Test error")
        {
            return CreateRunResult(
                action: action,
                parameter: parameter,
                exitCode: 1,
                stderr: errorMessage,
                durationSeconds: 2.0);
        }

        public static List<RunResult> CreateRunResultList(int count = 10, string action = "test")
        {
            var results = new List<RunResult>();
            for (int i = 0; i < count; i++)
            {
                results.Add(CreateRunResult(
                    action: action,
                    exitCode: i % 3 == 0 ? 1 : 0, // Every 3rd run fails
                    durationSeconds: Random.Next(1, 20)));
            }
            return results;
        }

        #endregion

        #region RunInfo

        public static RunInfo CreateRunInfo(string? fileName = null, DateTime? timestamp = null)
        {
            var ts = timestamp ?? DateTime.Now.AddHours(-Random.Next(0, 24));
            return new RunInfo
            {
                FilePath = $"/mock/runs/{fileName ?? $"run_{ts:yyyyMMdd_HHmmss}.json"}",
                DisplayName = fileName ?? $"run_{ts:yyyyMMdd_HHmmss}.json",
                Timestamp = ts
            };
        }

        public static List<RunInfo> CreateRunInfoList(int count = 10)
        {
            var runs = new List<RunInfo>();
            for (int i = 0; i < count; i++)
            {
                runs.Add(CreateRunInfo(timestamp: DateTime.Now.AddHours(-i)));
            }
            return runs;
        }

        #endregion

        #region RunMetric

        public static RunMetric CreateRunMetric(
            string action = "test",
            bool isSuccess = true,
            double durationSeconds = 5.0,
            int? qualityScore = null,
            DateTime? timestamp = null)
        {
            return new RunMetric
            {
                TimestampUtc = timestamp ?? DateTime.UtcNow.AddMinutes(-Random.Next(1, 1000)),
                Action = action,
                IsSuccess = isSuccess,
                DurationSeconds = durationSeconds,
                QualityScore = qualityScore ?? (isSuccess ? Random.Next(60, 100) : Random.Next(20, 60))
            };
        }

        public static List<RunMetric> CreateRunMetricList(int count = 10)
        {
            var metrics = new List<RunMetric>();
            for (int i = 0; i < count; i++)
            {
                metrics.Add(CreateRunMetric(
                    isSuccess: i % 4 != 0, // 75% success rate
                    action: i % 2 == 0 ? "test" : "improve"));
            }
            return metrics;
        }

        public static List<RunMetric> CreateTrendingMetrics(TrendDirection trend, int count = 10)
        {
            var metrics = new List<RunMetric>();

            var firstHalfCount = count / 2;
            var secondHalfCount = count - firstHalfCount;
            var (firstHalfSuccesses, secondHalfSuccesses) = trend switch
            {
                TrendDirection.Improving => (Math.Max(1, firstHalfCount - 2), secondHalfCount),
                TrendDirection.Declining => (firstHalfCount, Math.Min(1, secondHalfCount)),
                _ => (Math.Max(1, firstHalfCount / 2), Math.Max(1, secondHalfCount / 2)),
            };

            for (int i = 0; i < count; i++)
            {
                bool isSuccess = i < firstHalfCount
                    ? i >= firstHalfCount - firstHalfSuccesses
                    : (i - firstHalfCount) < secondHalfSuccesses;

                metrics.Add(CreateRunMetric(
                    isSuccess: isSuccess,
                    timestamp: DateTime.UtcNow.AddMinutes(-(count - i) * 10)));
            }

            return metrics;
        }

        #endregion

        #region SkillAnalytics

        public static SkillAnalytics CreateSkillAnalytics(
            string skillName = "test-skill",
            int totalRuns = 10,
            int successfulRuns = 8,
            double? averageQualityScore = 75.0,
            TrendDirection trend = TrendDirection.Stable,
            DateTime? lastRunAt = null)
        {
            var history = new List<RunMetric>();
            for (int i = 0; i < totalRuns; i++)
            {
                history.Add(CreateRunMetric(
                    isSuccess: i < successfulRuns,
                    timestamp: DateTime.UtcNow.AddHours(-(totalRuns - i))));
            }

            return new SkillAnalytics
            {
                SkillName = skillName,
                TotalRuns = totalRuns,
                SuccessfulRuns = successfulRuns,
                FailedRuns = totalRuns - successfulRuns,
                RunHistory = history,
                LastRunAtUtc = lastRunAt ?? DateTime.UtcNow.AddHours(-1),
                AverageQualityScore = averageQualityScore,
                Trend = trend
            };
        }

        public static List<SkillAnalytics> CreateSkillAnalyticsList(int count = 5)
        {
            var analytics = new List<SkillAnalytics>();
            for (int i = 0; i < count; i++)
            {
                var trend = (TrendDirection)(i % 3);
                var totalRuns = Random.Next(5, 50);
                var successRate = trend == TrendDirection.Declining ? 0.4 : 0.85;
                
                analytics.Add(CreateSkillAnalytics(
                    skillName: $"test-skill-{i}",
                    totalRuns: totalRuns,
                    successfulRuns: (int)(totalRuns * successRate),
                    trend: trend));
            }
            return analytics;
        }

        #endregion

        #region DetectedRuntime

        public static DetectedRuntime CreateDetectedRuntime(
            string name = "codex",
            string command = "codex",
            List<string>? models = null,
            bool isAvailable = true)
        {
            var availableCommand = Environment.ProcessPath ?? command;
            return new DetectedRuntime
            {
                Name = name,
                Command = isAvailable ? availableCommand : string.Empty,
                Models = models ?? new List<string> { "gpt-4", "gpt-3.5-turbo" }
            };
        }

        public static List<DetectedRuntime> CreateDetectedRuntimeList(int count = 3)
        {
            var runtimes = new List<DetectedRuntime>
            {
                CreateDetectedRuntime("codex", "codex", new List<string> { "gpt-4", "gpt-3.5-turbo" }, true),
                CreateDetectedRuntime("gemini", "gemini", new List<string> { "gemini-pro", "gemini-ultra" }, true),
                CreateDetectedRuntime("copilot", "copilot", new List<string> { "github-copilot" }, false)
            };

            return runtimes.GetRange(0, Math.Min(count, runtimes.Count));
        }

        #endregion

        #region AppConfiguration

        public static AppConfiguration CreateAppConfiguration(
            List<DetectedRuntime>? runtimes = null,
            Dictionary<string, RoleConfiguration>? roles = null)
        {
            var config = new AppConfiguration
            {
                Version = 1,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-7),
                LastUpdatedUtc = DateTime.UtcNow,
                DetectedRuntimes = runtimes ?? CreateDetectedRuntimeList(),
                Roles = roles ?? new Dictionary<string, RoleConfiguration>()
            };

            // Ensure all roles exist
            var defaultRoles = AppConfiguration.GetDefaultRoles();
            foreach (var (key, value) in defaultRoles)
            {
                if (!config.Roles.ContainsKey(key))
                {
                    config.Roles[key] = value;
                }
            }

            return config;
        }

        #endregion

        #region JudgeResult

        public static JudgeResult CreateJudgeResult(
            int? qualityScore = 85,
            string? routingNotes = "Good routing quality",
            string? behaviorNotes = "Expected behavior observed",
            List<string>? priorityFixes = null)
        {
            return new JudgeResult
            {
                QualityScore = qualityScore,
                RoutingNotes = routingNotes,
                BehaviorNotes = behaviorNotes,
                PriorityFixes = priorityFixes ?? new List<string> { "Fix A", "Fix B" }
            };
        }

        public static string CreateJudgeOutput(int qualityScore = 85)
        {
            return $@"
Judge Evaluation Report
======================

Quality Score: {qualityScore}/100

Routing Quality Notes:
The skill demonstrated good routing quality with accurate intent recognition.

Behavior Quality Notes:
The skill behavior matched expected patterns for the given input.

Highest Priority Fixes:
1. Improve error handling for edge cases
2. Add validation for user input
3. Optimize response time for large inputs
";
        }

        #endregion
    }
}
