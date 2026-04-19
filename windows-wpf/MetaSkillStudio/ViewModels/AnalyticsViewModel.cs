using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Commands;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// ViewModel for Analytics Dashboard with DI support.
    /// </summary>
    public class AnalyticsViewModel : INotifyPropertyChanged
    {
        private readonly IPythonRuntimeService _pythonService;
        private readonly IEnvironmentProvider _environmentProvider;
        private List<SkillAnalytics> _skillAnalytics = new();
        private List<AlertItem> _alerts = new();
        private bool _isLoading = false;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public AnalyticsViewModel(IPythonRuntimeService pythonService, IEnvironmentProvider environmentProvider)
        {
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
            _environmentProvider = environmentProvider ?? throw new ArgumentNullException(nameof(environmentProvider));
            RefreshCommand = new RelayCommand(async () => await LoadAnalyticsAsync(), () => !IsLoading);
        }

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether analytics data is currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of skill analytics data.
        /// </summary>
        public List<SkillAnalytics> SkillAnalytics
        {
            get => _skillAnalytics;
            set => SetProperty(ref _skillAnalytics, value);
        }

        /// <summary>
        /// Gets or sets the list of alert items for the analytics dashboard.
        /// </summary>
        public List<AlertItem> Alerts
        {
            get => _alerts;
            set => SetProperty(ref _alerts, value);
        }

        /// <summary>
        /// Gets the command to refresh analytics data.
        /// </summary>
        public ICommand RefreshCommand { get; }

        #endregion

        /// <summary>
        /// Loads analytics data from the runs directory asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAnalyticsAsync()
        {
            IsLoading = true;
            try
            {
                var runsDir = GetRunsDirectory();
                if (string.IsNullOrEmpty(runsDir) || !_environmentProvider.DirectoryExists(runsDir))
                {
                    Alerts = new List<AlertItem>
                    {
                        new AlertItem { Message = "No runs directory found. Run some operations first.", Severity = AlertSeverity.Warning }
                    };
                    return;
                }

                // Load all run files
                var runFiles = Directory.GetFiles(runsDir, "*.json")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .Take(200); // Limit to recent 200 runs

                var runs = new List<RunResult>();
                foreach (var file in runFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var run = JsonSerializer.Deserialize<RunResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (run != null)
                        {
                            runs.Add(run);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AnalyticsViewModel] Skipping corrupted run file: {ex.Message}");
                    }
                }

                // Aggregate by skill
                var analytics = AggregateAnalytics(runs);
                SkillAnalytics = analytics;

                // Generate alerts
                Alerts = GenerateAlerts(analytics);
            }
            catch (Exception ex)
            {
                Alerts = new List<AlertItem>
                {
                    new AlertItem { Message = $"Error loading analytics: {ex.Message}", Severity = AlertSeverity.Error }
                };
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<SkillAnalytics> AggregateAnalytics(List<RunResult> runs)
        {
            var skills = _pythonService.ListSkills();
            var analytics = new List<SkillAnalytics>();

            foreach (var skill in skills)
            {
                var skillRuns = runs.Where(r => 
                {
                    // Try to extract skill name from run
                    var skillParam = r.Input.GetValueOrDefault("skill")?.ToString() ?? "";
                    var briefParam = r.Input.GetValueOrDefault("brief")?.ToString() ?? "";
                    return skillParam == skill.Name || 
                           briefParam?.Contains(skill.Name, StringComparison.OrdinalIgnoreCase) == true ||
                           r.Stdout?.Contains(skill.Name, StringComparison.OrdinalIgnoreCase) == true;
                }).ToList();

                if (!skillRuns.Any())
                {
                    // Include skills with no runs (will show as "No data")
                    analytics.Add(new SkillAnalytics
                    {
                        SkillName = skill.Name,
                        TotalRuns = 0,
                        SuccessfulRuns = 0,
                        FailedRuns = 0,
                        RunHistory = new List<RunMetric>(),
                        LastRunAtUtc = null,
                        AverageQualityScore = null,
                        Trend = TrendDirection.Stable
                    });
                    continue;
                }

                var metrics = skillRuns.Select(r =>
                {
                    var judgeResult = _pythonService.ParseJudgeOutput(r.Stdout);
                    return new RunMetric
                    {
                        TimestampUtc = r.StartedAtUtc,
                        Action = r.Action,
                        IsSuccess = r.ExitCode == 0,
                        DurationSeconds = r.DurationSeconds,
                        QualityScore = judgeResult?.QualityScore
                    };
                }).OrderBy(m => m.TimestampUtc).ToList();

                var successfulRuns = metrics.Count(m => m.IsSuccess);
                var qualityScores = metrics.Where(m => m.QualityScore.HasValue).Select(m => m.QualityScore!.Value).ToList();

                analytics.Add(new SkillAnalytics
                {
                    SkillName = skill.Name,
                    TotalRuns = skillRuns.Count,
                    SuccessfulRuns = successfulRuns,
                    FailedRuns = skillRuns.Count - successfulRuns,
                    RunHistory = metrics,
                        LastRunAtUtc = metrics.LastOrDefault()?.TimestampUtc,
                    AverageQualityScore = qualityScores.Any() ? qualityScores.Average() : null,
                    Trend = CalculateTrend(metrics)
                });
            }

            return analytics.OrderByDescending(a => a.LastRunAtUtc).ToList();
        }

        private TrendDirection CalculateTrend(List<RunMetric> metrics)
        {
            if (metrics.Count < 3) return TrendDirection.Stable;

            // Compare first half vs second half success rates
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

        private List<AlertItem> GenerateAlerts(List<SkillAnalytics> analytics)
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
                if (skill.LastRunAtUtc == null || (DateTime.Now - skill.LastRunAtUtc.Value).TotalDays > 30)
                {
                    var days = skill.LastRunAtUtc == null ? "never" : $"{((DateTime.Now - skill.LastRunAtUtc.Value).TotalDays):F0} days ago";
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

        private string? GetRunsDirectory()
        {
            // Check environment variable first
            var metaHome = _environmentProvider.GetEnvironmentVariable("META_SKILL_STUDIO_HOME");
            if (!string.IsNullOrEmpty(metaHome))
            {
                var dir = _environmentProvider.CombinePaths(metaHome, "runs");
                if (_environmentProvider.DirectoryExists(dir)) return dir;
            }

            // Fallback to user profile
            var fallbackDir = _environmentProvider.CombinePaths(
                _environmentProvider.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".meta-skill-studio", "runs");
            
            return _environmentProvider.DirectoryExists(fallbackDir) ? fallbackDir : null;
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">Name of the property (automatically determined).</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Represents an alert item in the analytics dashboard.
    /// </summary>
    public class AlertItem
    {
        /// <summary>
        /// Gets or sets the name of the skill associated with the alert, if any.
        /// </summary>
        public string? SkillName { get; set; }

        /// <summary>
        /// Gets or sets the alert message text.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets the severity level of the alert.
        /// </summary>
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// Gets the color code for the alert severity.
        /// </summary>
        public string SeverityColor => Severity switch
        {
            AlertSeverity.Error => "#D32F2F",
            AlertSeverity.Warning => "#FF9800",
            AlertSeverity.Success => "#4CAF50",
            _ => "#757575"
        };
    }

    /// <summary>
    /// Defines the severity levels for analytics alerts.
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// Error-level severity indicating a critical issue.
        /// </summary>
        Error,

        /// <summary>
        /// Warning-level severity indicating a potential issue.
        /// </summary>
        Warning,

        /// <summary>
        /// Success-level severity indicating a positive status.
        /// </summary>
        Success,

        /// <summary>
        /// Information-level severity for general notifications.
        /// </summary>
        Info
    }
}
