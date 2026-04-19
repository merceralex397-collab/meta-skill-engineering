using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Commands;
using MetaSkillStudio.Extensions;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// Main view model with DI support for all dependencies.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPythonRuntimeService _pythonService;
        private readonly IDialogService _dialogService;
        private List<SkillInfo> _availableSkills = new();
        private string _outputText = "Welcome to Meta Skill Studio\n\nReady to manage your AI skills.\n\n" +
            "Configure runtimes via Settings button before first use.";
        private string _statusText = "Ready";
        private bool _isBusy = false;
        private RunInfo? _selectedRun;
        private string _runtimeStatus = "Detecting...";

        // PERFORMANCE FIX: Reusable StringBuilder to reduce allocations in AppendOutput
        private readonly StringBuilder _outputBuilder = new StringBuilder(4096);

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public MainViewModel(IPythonRuntimeService pythonService, IDialogService dialogService)
        {
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            // Initialize commands
            CreateSkillCommand = new RelayCommand(async () => await CreateSkillAsync(), () => !IsBusy);
            ImproveSkillCommand = new RelayCommand(async () => await ImproveSkillAsync(), () => !IsBusy);
            TestBenchmarkCommand = new RelayCommand(async () => await TestBenchmarkAsync(), () => !IsBusy);
            MetaManageCommand = new RelayCommand(async () => await MetaManageAsync(), () => !IsBusy);
            CreateBenchmarksCommand = new RelayCommand(async () => await CreateBenchmarksAsync(), () => !IsBusy);
            RefreshRunsCommand = new RelayCommand(async () => await RefreshRunsAsync(), () => !IsBusy);
            OpenSettingsCommand = new RelayCommand(async () => await OpenSettingsAsync(), () => !IsBusy);
            RefreshSkillsCommand = new RelayCommand(async () => await RefreshSkillsAsync(), () => !IsBusy);
            OpenAnalyticsCommand = new RelayCommand(async () => await OpenAnalyticsAsync(), () => !IsBusy);

            // Initial data load - SECURITY FIX: Use SafeFireAndForget to prevent unobserved exceptions
            InitializeAsync().SafeFireAndForget("MainViewModel.InitializeAsync");
        }

        #region Properties

        /// <summary>
        /// Gets or sets the output text displayed in the main window.
        /// </summary>
        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        /// <summary>
        /// Gets or sets the current status text displayed in the status bar.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Gets or sets the runtime status text indicating available AI CLI runtimes.
        /// </summary>
        public string RuntimeStatus
        {
            get => _runtimeStatus;
            set => SetProperty(ref _runtimeStatus, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// When true, commands are disabled.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected run from the run history.
        /// </summary>
        public RunInfo? SelectedRun
        {
            get => _selectedRun;
            set
            {
                if (SetProperty(ref _selectedRun, value) && value != null)
                {
                    LoadRunDetails(value);
                }
            }
        }

        /// <summary>
        /// Gets the collection of recent run history entries.
        /// </summary>
        public List<RunInfo> RunHistory { get; } = new();

        /// <summary>
        /// Gets or sets the list of available skills in the repository.
        /// </summary>
        public List<SkillInfo> AvailableSkills
        {
            get => _availableSkills;
            set => SetProperty(ref _availableSkills, value);
        }

        // Commands
        /// <summary>
        /// Gets the command to create a new skill.
        /// </summary>
        public ICommand CreateSkillCommand { get; }

        /// <summary>
        /// Gets the command to improve an existing skill.
        /// </summary>
        public ICommand ImproveSkillCommand { get; }

        /// <summary>
        /// Gets the command to test and benchmark skills.
        /// </summary>
        public ICommand TestBenchmarkCommand { get; }

        /// <summary>
        /// Gets the command to perform meta-management operations.
        /// </summary>
        public ICommand MetaManageCommand { get; }

        /// <summary>
        /// Gets the command to create benchmark cases for a skill.
        /// </summary>
        public ICommand CreateBenchmarksCommand { get; }

        /// <summary>
        /// Gets the command to refresh the run history list.
        /// </summary>
        public ICommand RefreshRunsCommand { get; }

        /// <summary>
        /// Gets the command to open the application settings dialog.
        /// </summary>
        public ICommand OpenSettingsCommand { get; }

        /// <summary>
        /// Gets the command to refresh the available skills list.
        /// </summary>
        public ICommand RefreshSkillsCommand { get; }

        /// <summary>
        /// Gets the command to open the analytics dashboard.
        /// </summary>
        public ICommand OpenAnalyticsCommand { get; }

        #endregion

        private async Task InitializeAsync()
        {
            await RefreshSkillsAsync();
            await RefreshRunsAsync();
            await UpdateRuntimeStatusAsync();
        }

        private async Task UpdateRuntimeStatusAsync()
        {
            try
            {
                var runtimes = await _pythonService.DetectRuntimesAsync();
                var availableCount = runtimes.Count(r => r.IsAvailable);
                RuntimeStatus = $"{availableCount} runtime(s) available";
            }
            catch
            {
                RuntimeStatus = "Runtime detection failed";
            }
        }

        private async Task RefreshSkillsAsync()
        {
            await ExecuteOperationAsync("Loading skills...", async () =>
            {
                AvailableSkills = _pythonService.ListSkills();
                AppendOutput($"Loaded {AvailableSkills.Count} skills from repository.");
            });
        }

        private async Task CreateSkillAsync()
        {
            var (result, skillBrief, targetLibrary) = _dialogService.ShowCreateSkillDialog();
            if (result == true)
            {
                await ExecuteOperationAsync("Creating skill...", async () =>
                {
                    var libraryName = targetLibrary == TargetLibrary.LibraryWorkbench ? "LibraryWorkbench" : "LibraryUnverified";
                    AppendOutput($"Creating skill in {libraryName}...");
                    
                    var runResult = await _pythonService.ExecuteCommandAsync("create", skillBrief, targetLibrary);
                    
                    AppendOutput($"Exit code: {runResult.ExitCode}");
                    AppendOutput(runResult.CombinedOutput);
                    
                    if (runResult.IsSuccess)
                    {
                        await RefreshRunsAsync();
                        await RefreshSkillsAsync();
                    }
                });
            }
        }

        private async Task ImproveSkillAsync()
        {
            if (!AvailableSkills.Any())
            {
                _dialogService.ShowMessage("No skills available. Please check repository connection.", "No Skills", MessageType.Warning);
                return;
            }

            var (skillResult, selectedSkill, _) = _dialogService.ShowSkillSelectionDialog(AvailableSkills, "Select the skill to improve:");
            if (skillResult != true || selectedSkill == null) return;

            var (goalResult, goalText) = _dialogService.ShowInputDialog("Improvement Goal", $"Enter improvement goal for {selectedSkill.Name}:", "");
            if (goalResult != true) return;

            await ExecuteOperationAsync("Improving skill...", async () =>
            {
                var parameter = $"{selectedSkill.Name}|{goalText}";
                var result = await _pythonService.ExecuteCommandAsync("improve", parameter);
                
                AppendOutput($"Exit code: {result.ExitCode}");
                AppendOutput(result.CombinedOutput);
                
                if (result.IsSuccess)
                {
                    await RefreshRunsAsync();
                }
            });
        }

        private async Task TestBenchmarkAsync()
        {
            if (!AvailableSkills.Any())
            {
                _dialogService.ShowMessage("No skills available. Please check repository connection.", "No Skills", MessageType.Warning);
                return;
            }

            var (skillResult, selectedSkill, testAllSkills) = _dialogService.ShowSkillSelectionDialog(AvailableSkills, "Select skill to test (or check 'Test all skills'):", allowTestAll: true);
            if (skillResult != true) return;

            string? skillName = testAllSkills ? null : selectedSkill?.Name;
            if (!testAllSkills && selectedSkill == null) return;

            await ExecuteOperationAsync("Running tests...", async () =>
            {
                if (skillName == null)
                {
                    AppendOutput("Testing all skills...");
                }
                else
                {
                    AppendOutput($"Testing skill: {skillName}");
                }
                
                var result = await _pythonService.ExecuteCommandAsync("test", skillName ?? "");
                
                AppendOutput($"Exit code: {result.ExitCode}");
                AppendOutput(result.CombinedOutput);
                
                // Try to parse judge output
                var judgeResult = _pythonService.ParseJudgeOutput(result.Stdout);
                if (judgeResult?.QualityScore.HasValue == true)
                {
                    AppendOutput($"Quality Score: {judgeResult.ScoreDisplay}");
                }
                
                if (result.IsSuccess)
                {
                    await RefreshRunsAsync();
                }
            });
        }

        private async Task MetaManageAsync()
        {
            var (result, responseText) = _dialogService.ShowInputDialog("Meta Manage", "Enter management objective:", "");
            if (result != true) return;

            await ExecuteOperationAsync("Managing meta-library...", async () =>
            {
                var runResult = await _pythonService.ExecuteCommandAsync("meta-manage", responseText);
                
                AppendOutput($"Exit code: {runResult.ExitCode}");
                AppendOutput(runResult.CombinedOutput);
                
                if (runResult.IsSuccess)
                {
                    await RefreshRunsAsync();
                    await RefreshSkillsAsync();
                }
            });
        }

        private async Task CreateBenchmarksAsync()
        {
            if (!AvailableSkills.Any())
            {
                _dialogService.ShowMessage("No skills available. Please check repository connection.", "No Skills", MessageType.Warning);
                return;
            }

            var (result, skillName, benchmarkGoal, caseCount) = _dialogService.ShowBenchmarkDialog();
            if (result != true) return;

            await ExecuteOperationAsync("Creating benchmarks...", async () =>
            {
                var parameter = $"{skillName}|{benchmarkGoal}";
                AppendOutput($"Creating {caseCount} benchmark cases for {skillName}...");
                
                var runResult = await _pythonService.ExecuteCommandAsync("benchmarks", parameter, TargetLibrary.LibraryWorkbench, caseCount);
                
                AppendOutput($"Exit code: {runResult.ExitCode}");
                AppendOutput(runResult.CombinedOutput);
                
                if (runResult.IsSuccess)
                {
                    await RefreshRunsAsync();
                }
            });
        }

        private async Task RefreshRunsAsync()
        {
            await ExecuteOperationAsync("Refreshing runs...", async () =>
            {
                RunHistory.Clear();
                
                string? runsDir = null;
                
                // Check environment variable first - Note: In real DI, this would come from IEnvironmentProvider
                var metaHome = Environment.GetEnvironmentVariable("META_SKILL_STUDIO_HOME");
                if (!string.IsNullOrEmpty(metaHome))
                {
                    runsDir = Path.Combine(metaHome, "runs");
                }
                
                // Fallback to user profile
                if (runsDir == null || !Directory.Exists(runsDir))
                {
                    runsDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                        ".meta-skill-studio", "runs");
                }
                
                if (Directory.Exists(runsDir))
                {
                    var files = Directory.GetFiles(runsDir, "*.json")
                        .OrderByDescending(f => File.GetLastWriteTime(f))
                        .Take(50);
                    
                    foreach (var file in files)
                    {
                        RunHistory.Add(new RunInfo 
                        { 
                            FilePath = file,
                            DisplayName = Path.GetFileName(file),
                            Timestamp = File.GetLastWriteTime(file)
                        });
                    }
                    
                    AppendOutput($"Loaded {RunHistory.Count} runs.");
                }
                else
                {
                    AppendOutput("No runs directory found.");
                }
                
                OnPropertyChanged(nameof(RunHistory));
            });
        }

        private async Task OpenSettingsAsync()
        {
            var result = _dialogService.ShowSettingsDialog();
            if (result == true)
            {
                await UpdateRuntimeStatusAsync();
                AppendOutput("Configuration updated successfully.");
            }
        }

        private async Task OpenAnalyticsAsync()
        {
            await Task.Run(() =>
            {
                _dialogService.ShowAnalyticsDialog();
            });
        }

        private void LoadRunDetails(RunInfo run)
        {
            _dialogService.ShowRunDetailsDialog(run);
        }

        private async Task ExecuteOperationAsync(string status, Func<Task> operation)
        {
            IsBusy = true;
            StatusText = status;
            try
            {
                await operation();
                StatusText = "Ready";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                AppendOutput($"ERROR: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AppendOutput(string text)
        {
            // PERFORMANCE FIX: Use StringBuilder for efficient string concatenation
            _outputBuilder.Clear();
            _outputBuilder.Append(OutputText);
            _outputBuilder.Append("\n\n[");
            _outputBuilder.Append(DateTime.Now.ToString("HH:mm:ss"));
            _outputBuilder.Append("] ");
            _outputBuilder.Append(text);

            // PERFORMANCE FIX: Limit output size to prevent unbounded memory growth (keep last 100KB)
            const int maxOutputLength = 100 * 1024;
            if (_outputBuilder.Length > maxOutputLength)
            {
                var startIndex = _outputBuilder.Length - maxOutputLength;
                // Find the next newline to start cleanly
                while (startIndex < _outputBuilder.Length && _outputBuilder[startIndex] != '\n')
                {
                    startIndex++;
                }
                OutputText = _outputBuilder.ToString(startIndex, _outputBuilder.Length - startIndex);
            }
            else
            {
                OutputText = _outputBuilder.ToString();
            }
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

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
