using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPythonRuntimeService _pythonService;
        private readonly IDialogService _dialogService;
        private readonly StringBuilder _outputBuilder = new(4096);

        private List<SkillInfo> _availableSkills = new();
        private List<SkillInfo> _filteredSkills = new();
        private List<LibrarySurfaceInfo> _librarySurfaces = new();
        private List<ChecklistItem> _startupChecklist = new();
        private List<HelpResourceInfo> _helpResources = new();
        private string _outputText = "Welcome to Meta Skill Studio\n\nReady to manage your AI skills.";
        private string _statusText = "Ready";
        private bool _isBusy;
        private bool _isAssistantBusy;
        private bool _isAssistantPanelOpen = true;
        private RunInfo? _selectedRun;
        private SkillInfo? _selectedLibrarySkill;
        private HelpResourceInfo? _selectedHelpResource;
        private string _runtimeStatus = "Checking runtime...";
        private string _runtimeStatusDisplay = "Checking...";
        private string _skillFilterText = string.Empty;
        private string _assistantPrompt = string.Empty;
        private string _assistantStatus = "Ready";
        private string _quickStartSummary = "Complete the checklist below to get started.";
        private int _librarySkillCount;

        public MainViewModel(IPythonRuntimeService pythonService, IDialogService dialogService)
        {
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            ChatHistory = new ObservableCollection<ChatMessage>();

            CreateSkillCommand = new RelayCommand(async () => await CreateSkillAsync(), () => !IsBusy);
            ImproveSkillCommand = new RelayCommand(async () => await ImproveSkillAsync(), () => !IsBusy);
            TestBenchmarkCommand = new RelayCommand(async () => await TestBenchmarkAsync(), () => !IsBusy);
            MetaManageCommand = new RelayCommand(async () => await MetaManageAsync(), () => !IsBusy);
            CreateBenchmarksCommand = new RelayCommand(async () => await CreateBenchmarksAsync(), () => !IsBusy);
            FindExternalSkillsCommand = new RelayCommand(async () => await FindExternalSkillsAsync(), () => !IsBusy);
            RefreshRunsCommand = new RelayCommand(async () => await RefreshRunsAsync(), () => !IsBusy);
            OpenSettingsCommand = new RelayCommand(async () => await OpenSettingsAsync(), () => !IsBusy);
            RefreshSkillsCommand = new RelayCommand(async () => await RefreshSkillsAsync(), () => !IsBusy);
            OpenAnalyticsCommand = new RelayCommand(async () => await OpenAnalyticsAsync(), () => !IsBusy);
            AssistantPromptCommand = new RelayCommand(async () => await SendAssistantPromptAsync(), () => !IsAssistantBusy && !string.IsNullOrWhiteSpace(AssistantPrompt));
            ToggleAssistantCommand = new RelayCommand(() =>
            {
                IsAssistantPanelOpen = !IsAssistantPanelOpen;
                return Task.CompletedTask;
            });
            OpenQuickStartCommand = new RelayCommand(() =>
            {
                OpenQuickStart();
                return Task.CompletedTask;
            }, () => !IsBusy);
            OpenSelectedHelpResourceCommand = new RelayCommand(() =>
            {
                OpenSelectedHelpResource();
                return Task.CompletedTask;
            }, () => !IsBusy && SelectedHelpResource?.IsAvailable == true);

            UpdateChecklistAndLibraryState();
            InitializeAsync().SafeFireAndForget("MainViewModel.InitializeAsync");
        }

        #region Properties

        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string RuntimeStatus
        {
            get => _runtimeStatus;
            set => SetProperty(ref _runtimeStatus, value);
        }

        public string RuntimeStatusDisplay
        {
            get => _runtimeStatusDisplay;
            set => SetProperty(ref _runtimeStatusDisplay, value);
        }

        public string SkillFilterText
        {
            get => _skillFilterText;
            set
            {
                if (SetProperty(ref _skillFilterText, value))
                {
                    ApplySkillFilter();
                }
            }
        }

        public string AssistantPrompt
        {
            get => _assistantPrompt;
            set
            {
                if (SetProperty(ref _assistantPrompt, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string AssistantStatus
        {
            get => _assistantStatus;
            set => SetProperty(ref _assistantStatus, value);
        }

        public string QuickStartSummary
        {
            get => _quickStartSummary;
            set => SetProperty(ref _quickStartSummary, value);
        }

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

        public bool IsAssistantBusy
        {
            get => _isAssistantBusy;
            set
            {
                if (SetProperty(ref _isAssistantBusy, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsAssistantPanelOpen
        {
            get => _isAssistantPanelOpen;
            set => SetProperty(ref _isAssistantPanelOpen, value);
        }

        public int LibrarySkillCount
        {
            get => _librarySkillCount;
            set => SetProperty(ref _librarySkillCount, value);
        }

        public string RunHistoryStatusText => RunHistory.Count > 0
            ? $"{RunHistory.Count} run artifacts available"
            : "No runs yet. Launch a workflow to start.";

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

        public SkillInfo? SelectedLibrarySkill
        {
            get => _selectedLibrarySkill;
            set
            {
                if (SetProperty(ref _selectedLibrarySkill, value))
                {
                    OnPropertyChanged(nameof(SelectedSkillSummary));
                }
            }
        }

        public List<RunInfo> RunHistory { get; } = new();

        public ObservableCollection<ChatMessage> ChatHistory { get; }

        public List<SkillInfo> AvailableSkills
        {
            get => _availableSkills;
            set
            {
                if (SetProperty(ref _availableSkills, value))
                {
                    ApplySkillFilter();
                }
            }
        }

        public List<SkillInfo> FilteredSkills
        {
            get => _filteredSkills;
            set => SetProperty(ref _filteredSkills, value);
        }

        public List<LibrarySurfaceInfo> LibrarySurfaces
        {
            get => _librarySurfaces;
            set => SetProperty(ref _librarySurfaces, value);
        }

        public List<ChecklistItem> StartupChecklist
        {
            get => _startupChecklist;
            set => SetProperty(ref _startupChecklist, value);
        }

        public string SelectedSkillSummary => SelectedLibrarySkill == null
            ? "Select a core skill to view details."
            : $"{SelectedLibrarySkill.DisplayName}\n{SelectedLibrarySkill.Description ?? "No description available."}";

        public List<HelpResourceInfo> HelpResources
        {
            get => _helpResources;
            set => SetProperty(ref _helpResources, value);
        }

        public HelpResourceInfo? SelectedHelpResource
        {
            get => _selectedHelpResource;
            set
            {
                if (SetProperty(ref _selectedHelpResource, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand CreateSkillCommand { get; }
        public ICommand ImproveSkillCommand { get; }
        public ICommand TestBenchmarkCommand { get; }
        public ICommand MetaManageCommand { get; }
        public ICommand CreateBenchmarksCommand { get; }
        public ICommand FindExternalSkillsCommand { get; }
        public ICommand RefreshRunsCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand RefreshSkillsCommand { get; }
        public ICommand OpenAnalyticsCommand { get; }
        public ICommand AssistantPromptCommand { get; }
        public ICommand ToggleAssistantCommand { get; }
        public ICommand OpenQuickStartCommand { get; }
        public ICommand OpenSelectedHelpResourceCommand { get; }

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
                var opencodeRuntime = runtimes.FirstOrDefault();
                if (opencodeRuntime?.IsAvailable == true)
                {
                    var activeModel = opencodeRuntime.Models.FirstOrDefault(model => !string.Equals(model, "auto", StringComparison.OrdinalIgnoreCase))
                        ?? opencodeRuntime.DefaultModel;
                    RuntimeStatus = string.Equals(activeModel, "auto", StringComparison.OrdinalIgnoreCase)
                        ? "AI Runtime Ready"
                        : $"AI Runtime Ready - {activeModel}";
                    RuntimeStatusDisplay = RuntimeStatus;
                }
                else
                {
                    RuntimeStatus = "AI runtime not detected";
                    RuntimeStatusDisplay = "Not detected";
                }
            }
            catch
            {
                RuntimeStatus = "Runtime detection failed";
                RuntimeStatusDisplay = "Detection failed";
            }
            finally
            {
                UpdateChecklistAndLibraryState();
            }
        }

        private async Task RefreshSkillsAsync()
        {
            await ExecuteOperationAsync("Loading skills...", async () =>
            {
                AvailableSkills = _pythonService.ListSkills();
                UpdateChecklistAndLibraryState();
                AppendOutput($"Loaded {AvailableSkills.Count} core skills.");
            });
        }

        private async Task CreateSkillAsync()
        {
            var (result, skillBrief, targetLibrary) = _dialogService.ShowCreateSkillDialog();
            if (result == true)
            {
                await ExecuteOperationAsync("Creating skill...", async () =>
                {
                    var libraryName = targetLibrary == TargetLibrary.LibraryWorkbench ? "Workbench" : "Skill Library";
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
            if (skillResult != true || selectedSkill == null)
            {
                return;
            }

            var (goalResult, goalText) = _dialogService.ShowInputDialog("Improvement Goal", $"Enter improvement goal for {selectedSkill.DisplayName}:", string.Empty);
            if (goalResult != true)
            {
                return;
            }

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
            if (skillResult != true)
            {
                return;
            }

            string? skillName = testAllSkills ? null : selectedSkill?.Name;
            if (!testAllSkills && selectedSkill == null)
            {
                return;
            }

            await ExecuteOperationAsync("Running tests...", async () =>
            {
                AppendOutput(skillName == null ? "Testing all skills..." : $"Testing skill: {skillName}");

                var result = await _pythonService.ExecuteCommandAsync("test", skillName ?? string.Empty);

                AppendOutput($"Exit code: {result.ExitCode}");
                AppendOutput(result.CombinedOutput);

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
            var (result, responseText) = _dialogService.ShowInputDialog("Library Audit", "Enter audit objective:", string.Empty);
            if (result != true)
            {
                return;
            }

            await ExecuteOperationAsync("Auditing library...", async () =>
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
            if (result != true)
            {
                return;
            }

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

        private async Task FindExternalSkillsAsync()
        {
            var (result, searchQuery) = _dialogService.ShowInputDialog("Find External Skills", "Enter search topic or description:", string.Empty);
            if (result != true)
            {
                return;
            }

            await ExecuteOperationAsync("Searching for community skills...", async () =>
            {
                var runResult = await _pythonService.ExecuteCommandAsync("find-skills", searchQuery);

                AppendOutput($"Exit code: {runResult.ExitCode}");
                AppendOutput(runResult.CombinedOutput);

                if (runResult.IsSuccess)
                {
                    await RefreshRunsAsync();
                }
            });
        }

        private async Task SendAssistantPromptAsync()
        {
            if (string.IsNullOrWhiteSpace(AssistantPrompt))
            {
                return;
            }

            var userMessage = AssistantPrompt.Trim();
            ChatHistory.Add(new ChatMessage { Role = "User", Content = userMessage });
            AssistantPrompt = string.Empty;

            // Build prompt with conversation history for context
            var promptBuilder = new StringBuilder();
            if (ChatHistory.Count > 1)
            {
                promptBuilder.AppendLine("Previous conversation context:");
                foreach (var msg in ChatHistory.SkipLast(1))
                {
                    promptBuilder.AppendLine($"{msg.Role}: {msg.Content}");
                }
                promptBuilder.AppendLine();
            }
            promptBuilder.AppendLine(userMessage);

            IsAssistantBusy = true;
            AssistantStatus = "Thinking...";

            try
            {
                var result = await _pythonService.ExecuteCommandAsync("assistant", promptBuilder.ToString());
                var responseContent = string.IsNullOrWhiteSpace(result.Stdout)
                    ? result.CombinedOutput
                    : result.Stdout.Trim();

                ChatHistory.Add(new ChatMessage { Role = "Assistant", Content = responseContent });
                AssistantStatus = result.IsSuccess ? "Ready" : "Request failed";

                AppendOutput(result.IsSuccess
                    ? "AI assistant completed a response."
                    : $"AI assistant failed: {result.Stderr}");
            }
            catch (Exception ex)
            {
                ChatHistory.Add(new ChatMessage { Role = "Assistant", Content = $"Error: {ex.Message}" });
                AssistantStatus = "Error";
                AppendOutput($"Assistant error: {ex.Message}");
            }
            finally
            {
                IsAssistantBusy = false;
            }
        }

        private async Task RefreshRunsAsync()
        {
            await ExecuteOperationAsync("Refreshing runs...", async () =>
            {
                RunHistory.Clear();

                string? runsDir = null;
                var metaHome = Environment.GetEnvironmentVariable("META_SKILL_STUDIO_HOME");
                if (!string.IsNullOrEmpty(metaHome))
                {
                    runsDir = Path.Combine(metaHome, "runs");
                }

                if (runsDir == null || !Directory.Exists(runsDir))
                {
                    runsDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".meta-skill-studio",
                        "runs");
                }

                if (Directory.Exists(runsDir))
                {
                    var files = Directory.GetFiles(runsDir, "*.json")
                        .OrderByDescending(File.GetLastWriteTime)
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
                    AppendOutput("No run history found yet.");
                }

                OnPropertyChanged(nameof(RunHistory));
                OnPropertyChanged(nameof(RunHistoryStatusText));
                UpdateChecklistAndLibraryState();
                await Task.CompletedTask;
            });
        }

        private async Task OpenSettingsAsync()
        {
            var result = _dialogService.ShowSettingsDialog();
            if (result == true)
            {
                await UpdateRuntimeStatusAsync();
                AppendOutput("Settings updated.");
            }
        }

        private async Task OpenAnalyticsAsync()
        {
            await Task.Run(() => _dialogService.ShowAnalyticsDialog());
        }

        private void OpenQuickStart()
        {
            var checklist = StartupChecklist.Any()
                ? string.Join(Environment.NewLine, StartupChecklist.Select(item => $"{item.StatusGlyph} {item.Title} - {item.Description}"))
                : "No checklist data available yet.";

            var message = $"Meta Skill Studio Quick Start{Environment.NewLine}{Environment.NewLine}" +
                          $"{QuickStartSummary}{Environment.NewLine}{Environment.NewLine}" +
                          $"{checklist}{Environment.NewLine}{Environment.NewLine}" +
                          $"{BuildHelpSummary()}{Environment.NewLine}{Environment.NewLine}" +
                          "Use the AI Assistant panel to ask questions, or launch a workflow from the dashboard.";

            _dialogService.ShowMessage(message, "Quick Start Guide", MessageType.Information);
        }

        private void OpenSelectedHelpResource()
        {
            if (SelectedHelpResource?.IsAvailable != true)
            {
                _dialogService.ShowMessage(
                    "Select an available documentation resource from the Help section first.",
                    "Help resource unavailable",
                    MessageType.Warning);
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = SelectedHelpResource.FullPath,
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
                AppendOutput($"Opened: {SelectedHelpResource.Title}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(
                    $"Could not open {SelectedHelpResource.Title}.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Help resource error",
                    MessageType.Error);
            }
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
            _outputBuilder.Clear();
            _outputBuilder.Append(OutputText);
            _outputBuilder.Append("\n\n[");
            _outputBuilder.Append(DateTime.Now.ToString("HH:mm:ss"));
            _outputBuilder.Append("] ");
            _outputBuilder.Append(text);

            const int maxOutputLength = 100 * 1024;
            if (_outputBuilder.Length > maxOutputLength)
            {
                var startIndex = _outputBuilder.Length - maxOutputLength;
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

        private void ApplySkillFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SkillFilterText)
                ? AvailableSkills
                : AvailableSkills
                    .Where(skill =>
                        skill.Name.Contains(SkillFilterText, StringComparison.OrdinalIgnoreCase) ||
                        skill.DisplayName.Contains(SkillFilterText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(skill.Description) &&
                         skill.Description.Contains(SkillFilterText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            FilteredSkills = filtered;

            if (SelectedLibrarySkill != null && !FilteredSkills.Contains(SelectedLibrarySkill))
            {
                SelectedLibrarySkill = null;
            }
        }

        private void UpdateChecklistAndLibraryState()
        {
            UpdateLibrarySurfaces();
            UpdateHelpResources();
            UpdateStartupChecklist();
        }

        private void UpdateLibrarySurfaces()
        {
            var repoRoot = ResolveRepoRoot();

            var libraryPath = Path.Combine(repoRoot, "LibraryUnverified");
            var workbenchPath = Path.Combine(repoRoot, "LibraryWorkbench");

            var libCount = CountLeafSkillDirectories(libraryPath);
            var wbCount = CountLeafSkillDirectories(workbenchPath);

            LibrarySkillCount = libCount;

            var surfaces = new List<LibrarySurfaceInfo>
            {
                BuildSurfaceInfo("Skill Library", "LibraryUnverified", "Extended skill library awaiting validation and curation.", CountImmediateDirectories(libraryPath), libCount),
            };

            if (wbCount > 0 || Directory.Exists(workbenchPath))
            {
                surfaces.Add(BuildSurfaceInfo("Workbench", "LibraryWorkbench", "Active workbench for testing and evaluation.", CountImmediateDirectories(workbenchPath), wbCount));
            }

            LibrarySurfaces = surfaces;
        }

        private void UpdateStartupChecklist()
        {
            var isRuntimeReady = RuntimeStatus.StartsWith("AI Runtime Ready", StringComparison.OrdinalIgnoreCase);
            var checklist = new List<ChecklistItem>
            {
                new ChecklistItem
                {
                    Title = "AI Runtime",
                    Description = isRuntimeReady
                        ? RuntimeStatus
                        : "AI runtime needs configuration before workflows can execute.",
                    IsComplete = isRuntimeReady,
                },
                new ChecklistItem
                {
                    Title = "Core Skills",
                    Description = $"{AvailableSkills.Count} core skill packages loaded.",
                    IsComplete = AvailableSkills.Count >= 17,
                },
                new ChecklistItem
                {
                    Title = "Skill Library",
                    Description = $"{LibrarySkillCount} skills available in the library.",
                    IsComplete = LibrarySkillCount > 100,
                },
                new ChecklistItem
                {
                    Title = "Run History",
                    Description = RunHistory.Count > 0
                        ? $"{RunHistory.Count} run artifacts available."
                        : "No run artifacts yet. Run a workflow to populate.",
                    IsComplete = RunHistory.Count > 0,
                },
            };

            StartupChecklist = checklist;
            QuickStartSummary = checklist.All(item => item.IsComplete)
                ? "All systems ready. Launch a workflow or chat with the AI assistant."
                : "Complete the remaining checklist items to get fully operational.";
        }

        private void UpdateHelpResources()
        {
            var repoRoot = ResolveRepoRoot();
            var helpResources = new List<HelpResourceInfo>
            {
                BuildHelpResource("Studio Guide", Path.Combine("windows-wpf", "README.md"), "Build, publish, and runtime setup for the Windows app.", repoRoot),
                BuildHelpResource("Project Overview", "README.md", "Root project overview, inventory, and workflow guidance.", repoRoot),
                BuildHelpResource("Troubleshooting", Path.Combine("docs", "troubleshooting.md"), "Fixes for runtime, build, and workflow issues.", repoRoot),
                BuildHelpResource("Architecture", Path.Combine("docs", "architecture.md"), "High-level architecture for the studio and workflow stack.", repoRoot),
                BuildHelpResource("Agent Guidance", "AGENTS.md", "Repository rules, skill structure, and inventory boundaries.", repoRoot),
            };

            HelpResources = helpResources;

            if (SelectedHelpResource == null || !helpResources.Contains(SelectedHelpResource))
            {
                SelectedHelpResource = helpResources.FirstOrDefault(resource => resource.IsAvailable) ?? helpResources.FirstOrDefault();
            }
        }

        private static LibrarySurfaceInfo BuildSurfaceInfo(string name, string relativePath, string description, int directoryCount, int skillCount)
        {
            return new LibrarySurfaceInfo
            {
                Name = name,
                RelativePath = relativePath,
                Description = description,
                DirectoryCount = directoryCount,
                SkillCount = skillCount,
            };
        }

        private static HelpResourceInfo BuildHelpResource(string title, string relativePath, string description, string repoRoot)
        {
            var fullPath = Path.Combine(repoRoot, relativePath);
            return new HelpResourceInfo
            {
                Title = title,
                RelativePath = relativePath,
                Description = description,
                FullPath = fullPath,
                IsAvailable = File.Exists(fullPath) || Directory.Exists(fullPath),
            };
        }

        private string BuildHelpSummary()
        {
            var resources = HelpResources.Where(resource => resource.IsAvailable).Take(4).ToList();
            if (resources.Count == 0)
            {
                return "Documentation: no bundled guides were detected.";
            }

            return "Documentation:" + Environment.NewLine +
                   string.Join(
                       Environment.NewLine,
                       resources.Select(resource => $"  - {resource.Title}"));
        }

        private string ResolveRepoRoot()
        {
            var firstSkillDirectory = AvailableSkills.FirstOrDefault()?.DirectoryPath;
            if (!string.IsNullOrWhiteSpace(firstSkillDirectory))
            {
                var parent = Directory.GetParent(firstSkillDirectory);
                if (parent != null)
                {
                    return parent.FullName;
                }
            }

            foreach (var candidate in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            {
                var current = candidate;
                while (!string.IsNullOrWhiteSpace(current))
                {
                    if (File.Exists(Path.Combine(current, "AGENTS.md")))
                    {
                        return current;
                    }

                    current = Path.GetDirectoryName(current);
                }
            }

            return Environment.CurrentDirectory;
        }

        private static int CountImmediateDirectories(string path)
        {
            return Directory.Exists(path) ? Directory.GetDirectories(path).Length : 0;
        }

        private static int CountSkillMarkers(string path)
        {
            return Directory.Exists(path) ? Directory.EnumerateFiles(path, "SKILL.md", SearchOption.AllDirectories).Count() : 0;
        }

        /// <summary>
        /// Counts leaf directories containing .md files, excluding known non-skill dirs.
        /// </summary>
        private static int CountLeafSkillDirectories(string path)
        {
            if (!Directory.Exists(path)) return 0;

            var excludedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "evals", "docs", ".git", "scripts", "node_modules", "__pycache__",
                "assets", "references", "agents", ".opencode"
            };

            int count = 0;
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    var dirName = Path.GetFileName(dir);
                    if (excludedNames.Contains(dirName)) continue;

                    var subdirs = Directory.GetDirectories(dir);
                    var hasNonExcludedSubdirs = subdirs.Any(sd => !excludedNames.Contains(Path.GetFileName(sd)));
                    if (hasNonExcludedSubdirs) continue;

                    var hasMdFiles = Directory.EnumerateFiles(dir, "*.md").Any();
                    if (hasMdFiles) count++;
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }

            return count;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
