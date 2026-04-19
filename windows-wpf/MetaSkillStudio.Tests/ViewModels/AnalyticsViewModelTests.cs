using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.ViewModels;
using MetaSkillStudio.Tests.Mocks;
using MetaSkillStudio.Tests.Helpers;
using Xunit;

namespace MetaSkillStudio.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for AnalyticsViewModel.
    /// Tests data aggregation and alert generation.
    /// </summary>
    public class AnalyticsViewModelTests
    {
        private readonly AnalyticsViewModel _viewModel;

        public AnalyticsViewModelTests()
        {
            _viewModel = new AnalyticsViewModel(
                new MockPythonRuntimeService(),
                new MockEnvironmentProvider());
        }

        #region Property Tests

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Assert
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.SkillAnalytics.Should().BeEmpty();
            _viewModel.Alerts.Should().BeEmpty();
        }

        [Fact]
        public void IsLoading_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AnalyticsViewModel.IsLoading))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.IsLoading = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.IsLoading.Should().BeTrue();
        }

        [Fact]
        public void SkillAnalytics_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            var analytics = TestDataGenerator.CreateSkillAnalyticsList();
            
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AnalyticsViewModel.SkillAnalytics))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.SkillAnalytics = analytics;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.SkillAnalytics.Should().BeEquivalentTo(analytics);
        }

        [Fact]
        public void Alerts_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            var alerts = new List<AlertItem>
            {
                new AlertItem { Message = "Test alert", Severity = AlertSeverity.Warning }
            };
            
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AnalyticsViewModel.Alerts))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.Alerts = alerts;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.Alerts.Should().BeEquivalentTo(alerts);
        }

        #endregion

        #region CalculateTrend Tests (via LoadAnalyticsAsync)

        [Fact]
        public void CalculateTrend_LessThan3Runs_ReturnsStable()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateRunMetricList(2);

            // Act - using reflection to test private method
            var method = typeof(AnalyticsViewModel).GetMethod("CalculateTrend",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { metrics });

            // Assert
            result.Should().Be(TrendDirection.Stable);
        }

        [Fact]
        public void CalculateTrend_WithImprovingSuccessRate_ReturnsImproving()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Improving, 10);

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("CalculateTrend",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { metrics });

            // Assert
            result.Should().Be(TrendDirection.Improving);
        }

        [Fact]
        public void CalculateTrend_WithDecliningSuccessRate_ReturnsDeclining()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Declining, 10);

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("CalculateTrend",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { metrics });

            // Assert
            result.Should().Be(TrendDirection.Declining);
        }

        [Fact]
        public void CalculateTrend_WithStableSuccessRate_ReturnsStable()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Stable, 10);

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("CalculateTrend",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { metrics });

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
                    successfulRuns: 9,
                    averageQualityScore: 85,
                    trend: TrendDirection.Stable,
                    lastRunAt: DateTime.Now.AddDays(-1))
            };

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            result.Should().ContainSingle();
            result![0].Message.Should().Contain("healthy");
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
                    successfulRuns: 3)
            };

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            result.Should().Contain(a => a.SkillName == "failing-skill" && a.Severity == AlertSeverity.Error);
        }

        [Fact]
        public void GenerateAlerts_WithNoRecentRuns_CreatesWarningAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "stale-skill",
                    lastRunAt: DateTime.Now.AddDays(-45))
            };

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            result.Should().Contain(a => a.SkillName == "stale-skill" && a.Severity == AlertSeverity.Warning);
        }

        [Fact]
        public void GenerateAlerts_WithDecliningTrend_CreatesWarningAlert()
        {
            // Arrange
            var metrics = TestDataGenerator.CreateTrendingMetrics(TrendDirection.Declining, 10);
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "declining-skill",
                    totalRuns: 10,
                    successfulRuns: 5)
            };
            analytics[0].Trend = TrendDirection.Declining;

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            result.Should().Contain(a => a.SkillName == "declining-skill" && a.Severity == AlertSeverity.Warning);
        }

        [Fact]
        public void GenerateAlerts_WithLowQualityScore_CreatesErrorAlert()
        {
            // Arrange
            var analytics = new List<SkillAnalytics>
            {
                TestDataGenerator.CreateSkillAnalytics(
                    skillName: "low-quality-skill",
                    averageQualityScore: 40)
            };

            // Act
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            result.Should().Contain(a => a.SkillName == "low-quality-skill" && a.Severity == AlertSeverity.Error);
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
            var method = typeof(AnalyticsViewModel).GetMethod("GenerateAlerts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_viewModel, new object[] { analytics }) as List<AlertItem>;

            // Assert
            var errorIndex = result!.FindIndex(a => a.Severity == AlertSeverity.Error);
            var warningIndex = result.FindIndex(a => a.Severity == AlertSeverity.Warning);
            errorIndex.Should().BeLessThan(warningIndex);
        }

        #endregion

        #region AlertItem Tests

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

        #region RefreshCommand Tests

        [Fact]
        public void RefreshCommand_IsInitialized()
        {
            // Assert
            _viewModel.RefreshCommand.Should().NotBeNull();
        }

        [Fact]
        public void RefreshCommand_CanExecute_WhenNotLoading()
        {
            // Arrange
            _viewModel.IsLoading = false;

            // Assert
            _viewModel.RefreshCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void RefreshCommand_CannotExecute_WhenLoading()
        {
            // Arrange
            _viewModel.IsLoading = true;

            // Assert
            _viewModel.RefreshCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion
    }
}
