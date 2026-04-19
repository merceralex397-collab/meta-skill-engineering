using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.Tests.Helpers;
using MetaSkillStudio.ViewModels;
using Xunit;

namespace MetaSkillStudio.Tests.Models
{
    /// <summary>
    /// Unit tests for model classes and their behavior.
    /// </summary>
    public class ModelTests
    {
        #region DetectedRuntime Tests

        [Fact]
        public void DetectedRuntime_IsAvailable_WhenCommandIsEmpty_ReturnsFalse()
        {
            // Arrange
            var runtime = new DetectedRuntime { Name = "test", Command = "" };

            // Assert
            runtime.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public void DetectedRuntime_DefaultModel_WhenModelsEmpty_ReturnsAuto()
        {
            // Arrange
            var runtime = new DetectedRuntime { Name = "test", Models = new System.Collections.Generic.List<string>() };

            // Assert
            runtime.DefaultModel.Should().Be("auto");
        }

        [Fact]
        public void DetectedRuntime_DefaultModel_WhenModelsPresent_ReturnsFirst()
        {
            // Arrange
            var runtime = new DetectedRuntime { Name = "test", Models = new System.Collections.Generic.List<string> { "gpt-4", "gpt-3.5" } };

            // Assert
            runtime.DefaultModel.Should().Be("gpt-4");
        }

        [Fact]
        public void DetectedRuntime_DisplayName_ContainsAvailabilityIndicator()
        {
            // Arrange
            var availableRuntime = new DetectedRuntime { Name = "codex", Command = System.Environment.ProcessPath! };
            var unavailableRuntime = new DetectedRuntime { Name = "gemini", Command = "" };

            // Assert
            availableRuntime.DisplayName.Should().Contain("✓");
            unavailableRuntime.DisplayName.Should().Contain("✗");
        }

        #endregion

        #region SkillInfo Tests

        [Fact]
        public void SkillInfo_DisplayName_WhenDescriptionNull_ReturnsNameOnly()
        {
            // Arrange
            var skill = new SkillInfo { Name = "test-skill", Description = null };

            // Assert
            skill.DisplayName.Should().Be("test-skill");
        }

        [Fact]
        public void SkillInfo_DisplayName_WhenDescriptionPresent_ReturnsNameAndPreview()
        {
            // Arrange
            var skill = new SkillInfo { Name = "test-skill", Description = "A test skill description" };

            // Assert
            skill.DisplayName.Should().Contain("test-skill");
            skill.DisplayName.Should().Contain("A test skill description");
        }

        [Fact]
        public void SkillInfo_DisplayName_TruncatesLongDescriptions()
        {
            // Arrange
            var longDescription = new string('a', 100);
            var skill = new SkillInfo { Name = "test-skill", Description = longDescription };

            // Assert
            skill.DisplayName.Should().EndWith("...");
            skill.DisplayName.Length.Should().BeLessThan(70); // Name + " - " + 50 chars + "..."
        }

        #endregion

        #region RunResult Tests

        [Fact]
        public void RunResult_IsSuccess_WhenExitCodeZero_ReturnsTrue()
        {
            // Arrange
            var result = new RunResult { ExitCode = 0 };

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void RunResult_IsSuccess_WhenExitCodeNonZero_ReturnsFalse()
        {
            // Arrange
            var result = new RunResult { ExitCode = 1 };

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void RunResult_CombinedOutput_ContainsStdoutAndStderr()
        {
            // Arrange
            var result = new RunResult { Stdout = "Standard output", Stderr = "Standard error" };

            // Assert
            result.CombinedOutput.Should().Contain("Standard output");
            result.CombinedOutput.Should().Contain("Standard error");
            result.CombinedOutput.Should().Contain("[STDERR]");
        }

        [Fact]
        public void RunResult_CombinedOutput_WhenStderrEmpty_DoesNotContainStderrSection()
        {
            // Arrange
            var result = new RunResult { Stdout = "Standard output", Stderr = "" };

            // Assert
            result.CombinedOutput.Should().Be("Standard output");
            result.CombinedOutput.Should().NotContain("[STDERR]");
        }

        #endregion

        #region JudgeResult Tests

        [Fact]
        public void JudgeResult_ScoreDisplay_WhenQualityScorePresent_FormatsCorrectly()
        {
            // Arrange
            var result = new JudgeResult { QualityScore = 85 };

            // Assert
            result.ScoreDisplay.Should().Be("85/100");
        }

        [Fact]
        public void JudgeResult_ScoreDisplay_WhenQualityScoreNull_ReturnsNA()
        {
            // Arrange
            var result = new JudgeResult { QualityScore = null };

            // Assert
            result.ScoreDisplay.Should().Be("N/A");
        }

        [Theory]
        [InlineData(85, "green")]
        [InlineData(60, "yellow")]
        [InlineData(50, "red")]
        [InlineData(90, "green")]
        public void JudgeResult_ScoreColorClass_ReturnsCorrectColor(int score, string expectedColor)
        {
            // Arrange
            var result = new JudgeResult { QualityScore = score };

            // Assert
            result.ScoreColorClass.Should().Be(expectedColor);
        }

        [Fact]
        public void JudgeResult_ScoreColorClass_WhenQualityScoreNull_ReturnsGray()
        {
            // Arrange
            var result = new JudgeResult { QualityScore = null };

            // Assert
            result.ScoreColorClass.Should().Be("gray");
        }

        #endregion

        #region SkillAnalytics Tests

        [Fact]
        public void SkillAnalytics_SuccessRate_WhenTotalRunsGreaterThanZero_CalculatesCorrectly()
        {
            // Arrange
            var analytics = new SkillAnalytics
            {
                TotalRuns = 10,
                SuccessfulRuns = 7
            };

            // Assert
            analytics.SuccessRate.Should().Be(70.0);
        }

        [Fact]
        public void SkillAnalytics_SuccessRate_WhenTotalRunsZero_ReturnsZero()
        {
            // Arrange
            var analytics = new SkillAnalytics
            {
                TotalRuns = 0,
                SuccessfulRuns = 0
            };

            // Assert
            analytics.SuccessRate.Should().Be(0);
        }

        [Fact]
        public void SkillAnalytics_FailedRuns_CalculatedCorrectly()
        {
            // Arrange
            var analytics = new SkillAnalytics
            {
                TotalRuns = 10,
                SuccessfulRuns = 7
            };

            // Assert
            analytics.FailedRuns.Should().Be(3);
        }

        #endregion

        #region RoleConfiguration Tests

        [Fact]
        public void RoleConfiguration_Label_ReturnsLabelFromRoleLabels()
        {
            // Arrange
            var config = new RoleConfiguration { Runtime = "create" };

            // Assert
            config.Label.Should().Be("Creating Skills");
        }

        [Fact]
        public void RoleConfiguration_Label_WhenUnknownRole_ReturnsRoleName()
        {
            // Arrange
            var config = new RoleConfiguration { Runtime = "unknown-role" };

            // Assert
            config.Label.Should().Be("unknown-role");
        }

        #endregion

        #region RoleLabels Tests

        [Theory]
        [InlineData("create", "Creating Skills")]
        [InlineData("improve", "Improving Skills")]
        [InlineData("test", "Testing / Benchmarking / Evaluating")]
        [InlineData("orchestrate", "Meta Management")]
        [InlineData("judge", "LLM Judge")]
        public void RoleLabels_GetLabel_ReturnsCorrectLabel(string role, string expectedLabel)
        {
            // Act & Assert
            RoleLabels.GetLabel(role).Should().Be(expectedLabel);
        }

        [Fact]
        public void RoleLabels_All_ContainsAllRoles()
        {
            // Assert
            RoleLabels.All.Should().ContainKey("create");
            RoleLabels.All.Should().ContainKey("improve");
            RoleLabels.All.Should().ContainKey("test");
            RoleLabels.All.Should().ContainKey("orchestrate");
            RoleLabels.All.Should().ContainKey("judge");
        }

        #endregion

        #region AppConfiguration Tests

        [Fact]
        public void AppConfiguration_GetDefaultRoles_ContainsAllRoles()
        {
            // Act
            var roles = AppConfiguration.GetDefaultRoles();

            // Assert
            roles.Should().ContainKey("create");
            roles.Should().ContainKey("improve");
            roles.Should().ContainKey("test");
            roles.Should().ContainKey("orchestrate");
            roles.Should().ContainKey("judge");
        }

        [Fact]
        public void AppConfiguration_DefaultRoles_HaveEmptyConfiguration()
        {
            // Act
            var roles = AppConfiguration.GetDefaultRoles();

            // Assert
            foreach (var role in roles.Values)
            {
                role.Runtime.Should().BeEmpty();
                role.Model.Should().Be("auto");
            }
        }

        #endregion

        #region AlertItem Tests

        [Theory]
        [InlineData(AlertSeverity.Error, "#D32F2F")]
        [InlineData(AlertSeverity.Warning, "#FF9800")]
        [InlineData(AlertSeverity.Success, "#4CAF50")]
        [InlineData(AlertSeverity.Info, "#757575")]
        public void AlertItem_SeverityColor_ReturnsCorrectHexColor(AlertSeverity severity, string expectedColor)
        {
            // Arrange
            var alert = new AlertItem { Severity = severity };

            // Assert
            alert.SeverityColor.Should().Be(expectedColor);
        }

        #endregion

        #region TestDataGenerator Integration

        [Fact]
        public void TestDataGenerator_CreateSkillList_GeneratesCorrectCount()
        {
            // Act
            var skills = TestDataGenerator.CreateSkillList(10);

            // Assert
            skills.Should().HaveCount(10);
            skills.Should().OnlyContain(s => !string.IsNullOrEmpty(s.Name));
        }

        [Fact]
        public void TestDataGenerator_CreateRunResultList_GeneratesMixedResults()
        {
            // Act
            var results = TestDataGenerator.CreateRunResultList(9);

            // Assert
            results.Should().HaveCount(9);
            // Every 3rd should fail (indices 0, 3, 6) - but that's exit code, not IsSuccess directly
            results.Count(r => r.ExitCode == 1).Should().Be(3); // 0, 3, 6
        }

        [Fact]
        public void TestDataGenerator_CreateSkillAnalytics_WithSpecifiedParameters()
        {
            // Arrange & Act
            var analytics = TestDataGenerator.CreateSkillAnalytics(
                skillName: "custom-skill",
                totalRuns: 20,
                successfulRuns: 15,
                averageQualityScore: 80);

            // Assert
            analytics.SkillName.Should().Be("custom-skill");
            analytics.TotalRuns.Should().Be(20);
            analytics.SuccessfulRuns.Should().Be(15);
            analytics.AverageQualityScore.Should().Be(80);
        }

        [Fact]
        public void TestDataGenerator_CreateTrendingMetrics_GeneratesCorrectTrend()
        {
            // Act
            var improving = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Improving, 10);
            var declining = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Declining, 10);
            var stable = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Stable, 10);

            // Assert - improving should have more successes in second half
            var improvingFirstHalfSuccess = improving.Take(5).Count(m => m.IsSuccess);
            var improvingSecondHalfSuccess = improving.Skip(5).Count(m => m.IsSuccess);
            improvingSecondHalfSuccess.Should().BeGreaterThan(improvingFirstHalfSuccess);

            // Declining should have more successes in first half
            var decliningFirstHalfSuccess = declining.Take(5).Count(m => m.IsSuccess);
            var decliningSecondHalfSuccess = declining.Skip(5).Count(m => m.IsSuccess);
            decliningFirstHalfSuccess.Should().BeGreaterThan(decliningSecondHalfSuccess);
        }

        #endregion
    }
}
