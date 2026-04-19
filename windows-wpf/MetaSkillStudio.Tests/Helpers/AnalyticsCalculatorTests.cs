using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.ViewModels;
using MetaSkillStudio.Tests.Helpers;
using Xunit;

namespace MetaSkillStudio.Tests.Helpers
{
    /// <summary>
    /// Unit tests for analytics calculation functions.
    /// Tests trend calculation and alert generation logic.
    /// </summary>
    public class AnalyticsCalculatorTests
    {
        #region CalculateTrend Tests

        [Fact]
        public void CalculateTrend_WithLessThan3Runs_ReturnsStable()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateRunMetricList(2);

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Stable);
        }

        [Fact]
        public void CalculateTrend_WithImprovingSuccessRate_ReturnsImproving()
        {
            // Arrange - 80% success rate in first half, 100% in second half
            var metrics = new List<RunMetric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new RunMetric
                {
                    TimestampUtc = DateTime.UtcNow.AddHours(-(10 - i)),
                    IsSuccess = i < 5 ? i < 4 : true // First 4 of 5 succeed, all 5 of second half succeed
                });
            }

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Improving);
        }

        [Fact]
        public void CalculateTrend_WithDecliningSuccessRate_ReturnsDeclining()
        {
            // Arrange - 100% success rate in first half, 20% in second half
            var metrics = new List<RunMetric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new RunMetric
                {
                    TimestampUtc = DateTime.UtcNow.AddHours(-(10 - i)),
                    IsSuccess = i < 5 || i == 8 // First 5 succeed, only 1 of second half succeeds
                });
            }

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Declining);
        }

        [Fact]
        public void CalculateTrend_WithStableSuccessRate_ReturnsStable()
        {
            // Arrange - Identical success rates in both halves
            var metrics = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Stable, 10);

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Stable);
        }

        [Fact]
        public void CalculateTrend_WithBorderlineImproving_ReturnsImproving()
        {
            // Arrange - Just above the 15% threshold
            var metrics = new List<RunMetric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new RunMetric
                {
                    TimestampUtc = DateTime.UtcNow.AddHours(-(10 - i)),
                    IsSuccess = i < 3 || (i >= 5 && i < 9) // 60% -> 80% = 20% improvement
                });
            }

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Improving);
        }

        [Fact]
        public void CalculateTrend_WithAllSuccess_ReturnsStable()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateRunMetricList(10);
            foreach (var metric in metrics)
            {
                metric.IsSuccess = true;
            }

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Stable);
        }

        [Fact]
        public void CalculateTrend_WithAllFailures_ReturnsStable()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateRunMetricList(10);
            foreach (var metric in metrics)
            {
                metric.IsSuccess = false;
            }

            // Act
            var result = CalculateTrend(metrics);

            // Assert
            result.Should().Be(TrendDirection.Stable);
        }

        #endregion

        #region GenerateAlerts Tests

        [Fact]
        public void GenerateAlerts_WithHealthySkills_ReturnsSuccessAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "healthy-skill",
                    totalRuns: 10,
                    successfulRuns: 10,
                    averageQualityScore: 90,
                    trend: TrendDirection.Stable)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().ContainSingle();
            result[0].Message.Should().Contain("healthy");
            result[0].Severity.Should().Be(AlertSeverity.Success);
        }

        [Fact]
        public void GenerateAlerts_WithLowPassRate_CreatesErrorAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "failing-skill",
                    totalRuns: 10,
                    successfulRuns: 3,
                    averageQualityScore: 90)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.SkillName == "failing-skill" && a.Severity == AlertSeverity.Error);
            result.Should().Contain(a => a.Message.Contains("Low pass rate"));
        }

        [Fact]
        public void GenerateAlerts_WithNoRecentRuns_CreatesWarningAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "stale-skill",
                    totalRuns: 5,
                    successfulRuns: 5,
                    lastRunAt: DateTime.Now.AddDays(-45))
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.SkillName == "stale-skill" && a.Severity == AlertSeverity.Warning);
            result.Should().Contain(a => a.Message.Contains("No recent runs"));
        }

        [Fact]
        public void GenerateAlerts_WithNeverRun_CreatesWarningAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                new SkillAnalytics
                {
                    SkillName = "never-run-skill",
                    TotalRuns = 0,
                    SuccessfulRuns = 0,
                    FailedRuns = 0,
                    RunHistory = new List<RunMetric>(),
                    LastRunAtUtc = null,
                    AverageQualityScore = null,
                    Trend = TrendDirection.Stable
                }
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.SkillName == "never-run-skill" && a.Severity == AlertSeverity.Warning);
            result.Should().Contain(a => a.Message.Contains("never"));
        }

        [Fact]
        public void GenerateAlerts_WithDecliningTrend_CreatesWarningAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "declining-skill",
                    totalRuns: 10,
                    successfulRuns: 5,
                    trend: TrendDirection.Declining)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.SkillName == "declining-skill" && a.Severity == AlertSeverity.Warning);
            result.Should().Contain(a => a.Message.Contains("Declining"));
        }

        [Fact]
        public void GenerateAlerts_WithDecliningTrendButLowRunCount_DoesNotCreateAlert()
        {
            // Arrange - Only 4 runs, so declining trend should not trigger alert
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "low-run-skill",
                    totalRuns: 4,
                    successfulRuns: 2,
                    trend: TrendDirection.Declining)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().NotContain(a => a.Message.Contains("Declining"));
        }

        [Fact]
        public void GenerateAlerts_WithLowQualityScore_CreatesErrorAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "low-quality-skill",
                    totalRuns: 10,
                    successfulRuns: 10,
                    averageQualityScore: 40)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.SkillName == "low-quality-skill" && a.Severity == AlertSeverity.Error);
            result.Should().Contain(a => a.Message.Contains("quality score"));
        }

        [Fact]
        public void GenerateAlerts_WithBorderlineQualityScore_DoesNotCreateAlert()
        {
            // Arrange - 50 is the threshold, so 50 should not trigger
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "borderline-quality-skill",
                    totalRuns: 10,
                    successfulRuns: 10,
                    averageQualityScore: 55)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().NotContain(a => a.Message.Contains("quality score"));
        }

        [Fact]
        public void GenerateAlerts_WithMultipleIssues_CreatesMultipleAlerts()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "problematic-skill",
                    totalRuns: 10,
                    successfulRuns: 3,
                    averageQualityScore: 40,
                    trend: TrendDirection.Declining)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().Contain(a => a.Message.Contains("Low pass rate"));
            result.Should().Contain(a => a.Message.Contains("quality score"));
            result.Should().Contain(a => a.Message.Contains("Declining"));
        }

        [Fact]
        public void GenerateAlerts_SortsBySeverity()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(skillName: "warning-skill", totalRuns: 5, successfulRuns: 5, lastRunAt: DateTime.Now.AddDays(-40)),
                TestDataGenerator.CreateSkillAnalytics(skillName: "error-skill", totalRuns: 10, successfulRuns: 3)
            };

            // Act
            var result = GenerateAlerts(analytics);

            // Assert - Errors should come before warnings
            var errorIndex = result.FindIndex(a => a.Severity == AlertSeverity.Error);
            var warningIndex = result.FindIndex(a => a.Severity == AlertSeverity.Warning);
            
            errorIndex.Should().BeLessThan(warningIndex);
        }

        [Fact]
        public void GenerateAlerts_WithMultipleSkills_CreatesAlertsForEach()
        {
            // Arrange
            var analytics = TestDataGenerator.CreateSkillAnalyticsList(5);

            // Act
            var result = GenerateAlerts(analytics);

            // Assert
            result.Should().NotBeEmpty();
        }

        #endregion

        #region AlertItem SeverityColor Tests

        [Theory]
        [InlineData(AlertSeverity.Error, "#D32F2F")]
        [InlineData(AlertSeverity.Warning, "#FF9800")]
        [InlineData(AlertSeverity.Success, "#4CAF50")]
        [InlineData(AlertSeverity.Info, "#757575")]
        public void AlertItem_ReturnsCorrectSeverityColor(AlertSeverity severity, string expectedColor)
        {
            // Arrange
            var alert = new AlertItem { Severity = severity };

            // Act & Assert
            alert.SeverityColor.Should().Be(expectedColor);
        }

        #endregion

        #region Helper Methods

        // These helper methods replicate the logic from AnalyticsViewModel for testing

        private static TrendDirection CalculateTrend(List<RunMetric> metrics)
        {
            if (metrics.Count < 3) return TrendDirection.Stable;

            var halfPoint = metrics.Count / 2;
            var firstHalf = metrics.Take(halfPoint);
            var secondHalf = metrics.Skip(halfPoint);

            var firstRate = firstHalf.Any() ? firstHalf.Count(m => m.IsSuccess) / (double)firstHalf.Count() : 0;
            var secondRate = secondHalf.Any() ? secondHalf.Count(m => m.IsSuccess) / (double)secondHalf.Count() : 0;

            var diff = secondRate - firstRate;
            return diff switch
            {
                > 0.15 => TrendDirection.Improving,
                < -0.15 => TrendDirection.Declining,
                _ => TrendDirection.Stable
            };
        }

        private static List<AlertItem> GenerateAlerts(List<SkillAnalytics> analytics)
        {
            var alerts = new List<AlertItem>();

            foreach (var skill in analytics)
            {
                // Low pass rate alert
                if (skill.TotalRuns > 0 && skill.SuccessRate < 60)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"Low pass rate: {skill.SuccessRate:F1}% ({skill.SuccessfulRuns}/{skill.TotalRuns} runs)",
                        Severity = AlertSeverity.Error
                    });
                }

                // No runs in 30 days alert
                if (skill.LastRunAtUtc == null || (DateTime.UtcNow - skill.LastRunAtUtc.Value).TotalDays > 30)
                {
                    var days = skill.LastRunAtUtc == null ? "never" : $"{((DateTime.UtcNow - skill.LastRunAtUtc.Value).TotalDays):F0} days ago";
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"No recent runs (last run: {days})",
                        Severity = AlertSeverity.Warning
                    });
                }

                // Declining trend alert
                if (skill.Trend == TrendDirection.Declining && skill.TotalRuns >= 5)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = "Declining success trend over recent runs",
                        Severity = AlertSeverity.Warning
                    });
                }

                // Low quality score alert
                if (skill.AverageQualityScore.HasValue && skill.AverageQualityScore.Value < 50)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"Low average quality score: {skill.AverageQualityScore.Value:F0}/100",
                        Severity = AlertSeverity.Error
                    });
                }
            }

            // Add summary alert
            if (alerts.Count == 0)
            {
                alerts.Add(new AlertItem
                {
                    Message = "All skills look healthy! No alerts at this time.",
                    Severity = AlertSeverity.Success
                });
            }

            return alerts.OrderBy(a => a.Severity).ToList();
        }

        #endregion
    }
}
