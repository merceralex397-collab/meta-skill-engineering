using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Commands;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// ViewModel for pipeline execution dialog with progress tracking.
    /// Manages pipeline configuration, execution, and phase state.
    /// </summary>
    public class PipelineViewModel : INotifyPropertyChanged
    {
        private readonly IPythonRuntimeService _pythonService;
        private string _repoRoot;
        private bool _isRunning = false;
        private bool _isConfigurable = true;
        private string _statusMessage = "Ready to start pipeline";
        private double _overallProgress = 0;
        private ObservableCollection<PhaseViewModel> _phases = new();
        private string _currentPhase = "";

        /// <summary>
        /// Initializes a new instance of the PipelineViewModel class.
        /// </summary>
        /// <param name="pythonService">The Python runtime service for executing pipeline scripts.</param>
        /// <exception cref="ArgumentNullException">Thrown when pythonService is null.</exception>
        public PipelineViewModel(IPythonRuntimeService pythonService)
        {
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
            _repoRoot = FindRepoRoot();

            StartPipelineCommand = new RelayCommand(async () => await StartPipelineAsync(), () => !IsRunning);
        }

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the pipeline is currently running.
        /// When set to true, IsConfigurable is automatically set to false.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    IsConfigurable = !value;
                    OnPropertyChanged(nameof(StartPipelineCommand));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration panel is enabled.
        /// This is automatically managed based on IsRunning state.
        /// </summary>
        public bool IsConfigurable
        {
            get => _isConfigurable;
            set => SetProperty(ref _isConfigurable, value);
        }

        /// <summary>
        /// Gets or sets the current status message displayed in the UI.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets the overall pipeline progress percentage (0-100).
        /// </summary>
        public double OverallProgress
        {
            get => _overallProgress;
            set => SetProperty(ref _overallProgress, value);
        }

        /// <summary>
        /// Gets the collection of pipeline phases for display.
        /// </summary>
        public ObservableCollection<PhaseViewModel> Phases
        {
            get => _phases;
            private set => SetProperty(ref _phases, value);
        }

        /// <summary>
        /// Gets or sets the name of the currently executing phase.
        /// </summary>
        public string CurrentPhase
        {
            get => _currentPhase;
            set => SetProperty(ref _currentPhase, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to start the pipeline execution.
        /// </summary>
        public ICommand StartPipelineCommand { get; }

        #endregion

        #region Pipeline Execution

        /// <summary>
        /// Starts the pipeline execution asynchronously.
        /// Executes the Python pipeline script with the provided configuration.
        /// </summary>
        /// <param name="pipelineType">The type of pipeline to execute (creation, improvement, library-management).</param>
        /// <param name="targetSkill">The target skill name, or null for new skills.</param>
        /// <param name="brief">The brief or goal description for the pipeline.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartPipelineAsync(string pipelineType = "creation", string? targetSkill = null, string? brief = null)
        {
            if (IsRunning) return;

            IsRunning = true;
            StatusMessage = "Starting pipeline...";
            OverallProgress = 0;
            Phases.Clear();

            try
            {
                // Build script path
                var scriptPath = Path.Combine(_repoRoot, "skill-orchestrator", "scripts", "run_pipeline.py");

                // Run pipeline using ArgumentList to prevent command injection
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    WorkingDirectory = _repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Use ArgumentList instead of string concatenation to prevent injection
                psi.ArgumentList.Add(scriptPath);
                psi.ArgumentList.Add("--create");
                psi.ArgumentList.Add(pipelineType);
                psi.ArgumentList.Add("--repo-root");
                psi.ArgumentList.Add(_repoRoot);

                if (!string.IsNullOrEmpty(targetSkill))
                {
                    psi.ArgumentList.Add("--skill");
                    psi.ArgumentList.Add(targetSkill);
                }
                if (!string.IsNullOrEmpty(brief))
                {
                    psi.ArgumentList.Add("--brief");
                    psi.ArgumentList.Add(brief);
                }

                using var process = Process.Start(psi);
                if (process == null) throw new InvalidOperationException("Failed to start pipeline");

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Parse result
                bool success = process.ExitCode == 0;
                StatusMessage = success ? "Pipeline completed" : "Pipeline failed";

                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<PipelineResult>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null)
                        {
                            UpdatePhaseDisplay(result);
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore parse errors - output may not be JSON
                    }
                }

                OverallProgress = success ? 100 : 50;
            }
            catch (Exception ex)
            {
                // Route error to UI and log
                StatusMessage = $"Error: {ex.Message}";
                Debug.WriteLine($"[PipelineViewModel] StartPipelineAsync error: {ex}");
                OverallProgress = 0;
            }
            finally
            {
                IsRunning = false;
            }
        }

        private void UpdatePhaseDisplay(PipelineResult result)
        {
            // Update phases collection based on result
            Phases.Clear();

            // Create phase entries for executed phases
            for (int i = 0; i < result.PhasesExecuted; i++)
            {
                var phase = new PhaseViewModel
                {
                    SkillName = $"Phase {i + 1}",
                    StatusIcon = i < result.PhasesSuccessful ? "✓" : "✗",
                    StatusText = i < result.PhasesSuccessful ? "Completed" : "Failed",
                    IsRunning = false,
                    Progress = i < result.PhasesSuccessful ? 100 : 0,
                    ShowProgress = true
                };
                Phases.Add(phase);
            }

            OverallProgress = result.PhasesFailed > 0 ? 50 : 100;
        }

        private static string FindRepoRoot()
        {
            foreach (var candidate in new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory,
            })
            {
                var repoRoot = TryFindRepoRoot(candidate);
                if (!string.IsNullOrEmpty(repoRoot))
                {
                    return repoRoot;
                }
            }

            return Directory.GetCurrentDirectory();
        }

        private static string? TryFindRepoRoot(string? startPath)
        {
            var current = NormalizeDirectoryCandidate(startPath);

            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "AGENTS.md")))
                {
                    return current;
                }

                current = Path.GetDirectoryName(current);
            }

            return null;
        }

        private static string? NormalizeDirectoryCandidate(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Represents a single phase in the pipeline execution.
    /// </summary>
    public class PhaseViewModel : INotifyPropertyChanged
    {
        private string _skillName = "";
        private string _statusIcon = "○";
        private string _statusText = "Pending";
        private bool _isRunning = false;
        private double _progress = 0;
        private bool _showProgress = false;

        /// <summary>
        /// Gets or sets the name of the skill for this phase.
        /// </summary>
        public string SkillName
        {
            get => _skillName;
            set => SetProperty(ref _skillName, value);
        }

        /// <summary>
        /// Gets or sets the status icon (e.g., "○", "●", "✓", "✗").
        /// </summary>
        public string StatusIcon
        {
            get => _statusIcon;
            set => SetProperty(ref _statusIcon, value);
        }

        /// <summary>
        /// Gets or sets the status text (e.g., "Pending", "Running", "Completed", "Failed").
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this phase is currently running.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        /// <summary>
        /// Gets or sets the progress percentage (0-100).
        /// </summary>
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the progress bar.
        /// </summary>
        public bool ShowProgress
        {
            get => _showProgress;
            set => SetProperty(ref _showProgress, value);
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

    /// <summary>
    /// Represents the result of a pipeline execution.
    /// </summary>
    public class PipelineResult
    {
        /// <summary>
        /// Gets or sets the unique identifier for the pipeline execution.
        /// </summary>
        public string PipelineId { get; set; } = "";

        /// <summary>
        /// Gets or sets the overall status of the pipeline (e.g., "completed", "failed").
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Gets or sets the number of phases that were executed.
        /// </summary>
        public int PhasesExecuted { get; set; }

        /// <summary>
        /// Gets or sets the number of phases that completed successfully.
        /// </summary>
        public int PhasesSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the number of phases that failed.
        /// </summary>
        public int PhasesFailed { get; set; }
    }
}
