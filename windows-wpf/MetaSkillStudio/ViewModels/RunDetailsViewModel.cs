using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Clipboard = System.Windows.Clipboard;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// ViewModel for the RunDetailsDialog that manages run data loading,
    /// security validation, judge result parsing, and clipboard operations.
    /// </summary>
    public class RunDetailsViewModel : ObservableModel
    {
        private readonly IPythonRuntimeService _pythonService;
        private readonly RunInfo _runInfo;
        private RunResult? _runResult;
        private JudgeResult? _judgeResult;

        // UI Properties
        private string _runTitle = string.Empty;
        private string _runTimestamp = string.Empty;
        private string _statusText = string.Empty;
        private Brush _statusColor = Brushes.Gray;
        private string _qualityScore = string.Empty;
        private string _qualityLabel = string.Empty;
        private string _exitCodeText = string.Empty;
        private Brush _exitCodeColor = Brushes.Black;
        private string _durationText = string.Empty;
        private string _startedText = string.Empty;
        private string _endedText = string.Empty;
        private string _inputText = string.Empty;
        private string _stdoutText = string.Empty;
        private string _stderrText = string.Empty;
        private string _artifactsText = string.Empty;
        private string _routingNotesText = string.Empty;
        private string _priorityFixesText = string.Empty;
        private Brush _qualityScoreColor = Brushes.Gray;
        private Visibility _judgeSectionVisibility = Visibility.Collapsed;
        private Visibility _stderrSectionVisibility = Visibility.Collapsed;

        /// <summary>
        /// Initializes a new instance of the RunDetailsViewModel class.
        /// </summary>
        /// <param name="runInfo">The run information to display.</param>
        /// <param name="pythonService">The Python runtime service for parsing judge output.</param>
        /// <exception cref="ArgumentNullException">Thrown when runInfo or pythonService is null.</exception>
        public RunDetailsViewModel(RunInfo runInfo, IPythonRuntimeService pythonService)
        {
            _runInfo = runInfo ?? throw new ArgumentNullException(nameof(runInfo));
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
            LoadRunData();
        }

        #region Public Properties

        /// <summary>
        /// Gets the title of the run displayed in the dialog header.
        /// </summary>
        public string RunTitle
        {
            get => _runTitle;
            private set => SetProperty(ref _runTitle, value);
        }

        /// <summary>
        /// Gets the timestamp of the run displayed in the dialog header.
        /// </summary>
        public string RunTimestamp
        {
            get => _runTimestamp;
            private set => SetProperty(ref _runTimestamp, value);
        }

        /// <summary>
        /// Gets the status text for the status badge (SUCCESS or FAILED).
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Gets the background color brush for the status badge.
        /// </summary>
        public Brush StatusColor
        {
            get => _statusColor;
            private set => SetProperty(ref _statusColor, value);
        }

        /// <summary>
        /// Gets the quality score value for display in the judge score badge.
        /// </summary>
        public string QualityScore
        {
            get => _qualityScore;
            private set => SetProperty(ref _qualityScore, value);
        }

        /// <summary>
        /// Gets the quality label based on the judge score.
        /// </summary>
        public string QualityLabel
        {
            get => _qualityLabel;
            private set => SetProperty(ref _qualityLabel, value);
        }

        /// <summary>
        /// Gets the exit code text for display in the metadata section.
        /// </summary>
        public string ExitCodeText
        {
            get => _exitCodeText;
            private set => SetProperty(ref _exitCodeText, value);
        }

        /// <summary>
        /// Gets the color brush for the exit code text (green for success, red for failure).
        /// </summary>
        public Brush ExitCodeColor
        {
            get => _exitCodeColor;
            private set => SetProperty(ref _exitCodeColor, value);
        }

        /// <summary>
        /// Gets the duration text formatted for display.
        /// </summary>
        public string DurationText
        {
            get => _durationText;
            private set => SetProperty(ref _durationText, value);
        }

        /// <summary>
        /// Gets the started timestamp text.
        /// </summary>
        public string StartedText
        {
            get => _startedText;
            private set => SetProperty(ref _startedText, value);
        }

        /// <summary>
        /// Gets the ended timestamp text.
        /// </summary>
        public string EndedText
        {
            get => _endedText;
            private set => SetProperty(ref _endedText, value);
        }

        /// <summary>
        /// Gets the formatted input parameters text.
        /// </summary>
        public string InputText
        {
            get => _inputText;
            private set => SetProperty(ref _inputText, value);
        }

        /// <summary>
        /// Gets the standard output text.
        /// </summary>
        public string StdoutText
        {
            get => _stdoutText;
            private set => SetProperty(ref _stdoutText, value);
        }

        /// <summary>
        /// Gets the standard error text.
        /// </summary>
        public string StderrText
        {
            get => _stderrText;
            private set => SetProperty(ref _stderrText, value);
        }

        /// <summary>
        /// Gets the formatted artifacts text.
        /// </summary>
        public string ArtifactsText
        {
            get => _artifactsText;
            private set => SetProperty(ref _artifactsText, value);
        }

        /// <summary>
        /// Gets the routing notes text for the judge results section.
        /// </summary>
        public string RoutingNotesText
        {
            get => _routingNotesText;
            private set => SetProperty(ref _routingNotesText, value);
        }

        /// <summary>
        /// Gets the priority fixes text for the judge results section.
        /// </summary>
        public string PriorityFixesText
        {
            get => _priorityFixesText;
            private set => SetProperty(ref _priorityFixesText, value);
        }

        /// <summary>
        /// Gets the background color brush for the quality score badge.
        /// </summary>
        public Brush QualityScoreColor
        {
            get => _qualityScoreColor;
            private set => SetProperty(ref _qualityScoreColor, value);
        }

        /// <summary>
        /// Gets the visibility of the judge score section.
        /// </summary>
        public Visibility JudgeSectionVisibility
        {
            get => _judgeSectionVisibility;
            private set => SetProperty(ref _judgeSectionVisibility, value);
        }

        /// <summary>
        /// Gets the visibility of the stderr section.
        /// </summary>
        public Visibility StderrSectionVisibility
        {
            get => _stderrSectionVisibility;
            private set => SetProperty(ref _stderrSectionVisibility, value);
        }

        /// <summary>
        /// Gets the underlying RunResult model.
        /// </summary>
        public RunResult? RunResult => _runResult;

        #endregion

        /// <summary>
        /// Loads the run data from the file system with security validation.
        /// </summary>
        /// <exception cref="SecurityException">Thrown when path traversal is detected.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the run file does not exist.</exception>
        private void LoadRunData()
        {
            RunTitle = $"Run: {_runInfo.DisplayName}";
            RunTimestamp = $"Timestamp: {_runInfo.Timestamp:yyyy-MM-dd HH:mm:ss}";

            // Validate file path to prevent directory traversal attacks
            var fullPath = Path.GetFullPath(_runInfo.FilePath);
            var runsDirectory = GetRunsDirectory();

            // Ensure the requested file is within the runs directory
            if (!fullPath.StartsWith(runsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException("Invalid file path: Path traversal detected.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Run file not found: {_runInfo.FilePath}");
            }

            var json = File.ReadAllText(fullPath);
            _runResult = JsonSerializer.Deserialize<RunResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (_runResult == null) return;

            UpdateStatusBadge();
            UpdateMetadata();
            PopulateSections();
            ParseAndDisplayJudgeResults();
        }

        /// <summary>
        /// Gets the runs directory path from environment or default location.
        /// </summary>
        /// <returns>The full path to the runs directory.</returns>
        private static string GetRunsDirectory()
        {
            var studioHome = Environment.GetEnvironmentVariable("META_SKILL_STUDIO_HOME")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".meta-skill-studio");
            return Path.GetFullPath(Path.Combine(studioHome, "runs"));
        }

        /// <summary>
        /// Updates the status badge properties based on the run result.
        /// </summary>
        private void UpdateStatusBadge()
        {
            if (_runResult == null) return;

            StatusText = _runResult.StatusText;

            if (_runResult.ExitCode == 0)
            {
                StatusColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            }
            else
            {
                StatusColor = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
            }
        }

        /// <summary>
        /// Updates the metadata section properties.
        /// </summary>
        private void UpdateMetadata()
        {
            if (_runResult == null) return;

            ExitCodeText = _runResult.ExitCode.ToString();
            ExitCodeColor = _runResult.ExitCode == 0 ? Brushes.Green : Brushes.Red;
            DurationText = $"{_runResult.DurationSeconds:F2}s";
            StartedText = _runResult.StartedAtUtc.ToString();
            EndedText = _runResult.EndedAtUtc.ToString();
        }

        /// <summary>
        /// Populates the input, output, and artifacts sections.
        /// </summary>
        private void PopulateSections()
        {
            if (_runResult == null) return;

            InputText = FormatJson(_runResult.Input);
            StdoutText = _runResult.Stdout;
            ArtifactsText = FormatJson(_runResult.Artifacts);

            // Show stderr section only if there's content
            if (!string.IsNullOrWhiteSpace(_runResult.Stderr))
            {
                StderrSectionVisibility = Visibility.Visible;
                StderrText = _runResult.Stderr;
            }
            else
            {
                StderrSectionVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Parses judge output and updates the judge results section.
        /// </summary>
        private void ParseAndDisplayJudgeResults()
        {
            if (_runResult == null) return;

            _judgeResult = _pythonService.ParseJudgeOutput(_runResult.Stdout);

            if (_judgeResult?.QualityScore.HasValue != true)
            {
                JudgeSectionVisibility = Visibility.Collapsed;
                return;
            }

            var qualityScore = _judgeResult.QualityScore!.Value;
            JudgeSectionVisibility = Visibility.Visible;
            QualityScore = qualityScore.ToString();
            QualityLabel = _judgeResult.QualityLabel;

            // Color code the score badge based on score
            var score = qualityScore;
            if (score >= 80)
            {
                QualityScoreColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            }
            else if (score >= 60)
            {
                QualityScoreColor = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
            }
            else
            {
                QualityScoreColor = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
            }

            // Display notes
            RoutingNotesText = !string.IsNullOrWhiteSpace(_judgeResult.RoutingNotes)
                ? _judgeResult.RoutingNotes
                : "No specific routing notes provided.";

            if (_judgeResult.PriorityFixes?.Any() == true)
            {
                PriorityFixesText = string.Join("\n• ", _judgeResult.PriorityFixes.Select(f => $"• {f}"));
            }
            else
            {
                PriorityFixesText = "No priority fixes identified.";
            }
        }

        /// <summary>
        /// Formats an object as indented JSON with snake_case property naming.
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <returns>A formatted JSON string or "{}" if null.</returns>
        private static string FormatJson(object? obj)
        {
            if (obj == null) return "{}";
            try
            {
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
            }
            catch
            {
                return obj.ToString() ?? "";
            }
        }

        /// <summary>
        /// Copies all run details to the clipboard in a formatted text format.
        /// </summary>
        public void CopyToClipboard()
        {
            if (_runResult == null) return;

            // Pre-size StringBuilder with estimated capacity for performance
            var estimatedCapacity = 500 +
                (_runResult.Input?.ToString()?.Length ?? 0) +
                (_runResult.Stdout?.Length ?? 0) +
                (_runResult.Stderr?.Length ?? 0) +
                (_runResult.Artifacts?.ToString()?.Length ?? 0);

            var sb = new StringBuilder(estimatedCapacity);

            sb.AppendLine("RUN DETAILS");
            sb.AppendLine("===========");
            sb.Append("Action: ").AppendLine(_runResult.Action);
            sb.Append("Exit Code: ").AppendLine(_runResult.ExitCode.ToString());
            sb.Append("Duration: ").Append(_runResult.DurationSeconds.ToString("F2")).AppendLine("s");
            sb.Append("Started: ").AppendLine(_runResult.StartedAtUtc.ToString());
            sb.Append("Ended: ").AppendLine(_runResult.EndedAtUtc.ToString());
            sb.AppendLine();
            sb.AppendLine("INPUT:");
            sb.AppendLine(FormatJson(_runResult.Input));
            sb.AppendLine();
            sb.AppendLine("STDOUT:");
            sb.AppendLine(_runResult.Stdout);
            sb.AppendLine();
            sb.AppendLine("STDERR:");
            sb.AppendLine(_runResult.Stderr);
            sb.AppendLine();
            sb.AppendLine("ARTIFACTS:");
            sb.Append(FormatJson(_runResult.Artifacts));

            Clipboard.SetText(sb.ToString());
        }
    }
}
