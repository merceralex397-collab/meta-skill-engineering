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
        private StudioPage _selectedPage = StudioPage.Dashboard;
        private List<LibrarySkillEntry> _libraryEntries = new();
        private List<LibrarySkillEntry> _filteredLibraryEntries = new();
        private List<LibraryCategory> _libraryCategories = new();
        private TargetLibrary _selectedLibraryTier = TargetLibrary.LibraryUnverified;
        private string _selectedLibraryCategory = string.Empty;
        private string _librarySearchText = string.Empty;
        private LibrarySkillEntry? _selectedLibraryEntry;
        private int _libraryUnverifiedCount;
        private int _libraryTestingCount;
        private int _libraryVerifiedCount;
        private string _selectedLibrarySkillContent = string.Empty;
        private string _importPath = string.Empty;
        private string _importStatus = string.Empty;
        private string _createSkillName = string.Empty;
        private string _createSkillDescription = string.Empty;
        private string _improveSkillGoal = string.Empty;
        private SkillInfo? _improveSelectedSkill;
        private string _testStatus = string.Empty;
        private int _automationThreshold = 70;
        private int _automationMaxIterations = 5;
        private bool _automationRunning;
        private string _automationStatus = string.Empty;

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
            OpenSettingsCommand = new RelayCommand(async () => { SelectedPage = StudioPage.Settings; await Task.CompletedTask; });
            RefreshSkillsCommand = new RelayCommand(async () => await RefreshSkillsAsync(), () => !IsBusy);
            OpenAnalyticsCommand = new RelayCommand(async () => { SelectedPage = StudioPage.Analytics; await Task.CompletedTask; });
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

            // Navigation commands
            NavigateCommand = new RelayCommand<StudioPage>(page =>
            {
                SelectedPage = page;
                return Task.CompletedTask;
            });
            ClearChatCommand = new RelayCommand(() =>
            {
                ChatHistory.Clear();
                AssistantStatus = "Ready";
                return Task.CompletedTask;
            });
            RefreshLibraryCommand = new RelayCommand(async () => await RefreshLibraryAsync(), () => !IsBusy);
            PromoteSkillCommand = new RelayCommand(async () => await PromoteSkillAsync(), () => !IsBusy && SelectedLibraryEntry != null);
            DemoteSkillCommand = new RelayCommand(async () => await DemoteSkillAsync(), () => !IsBusy && SelectedLibraryEntry != null);
            ImportFromFolderCommand = new RelayCommand(async () => await ImportFromFolderAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(ImportPath));
            InlineCreateSkillCommand = new RelayCommand(async () => await InlineCreateSkillAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(CreateSkillDescription));
            InlineImproveSkillCommand = new RelayCommand(async () => await InlineImproveSkillAsync(), () => !IsBusy && ImproveSelectedSkill != null);
            InlineTestSkillCommand = new RelayCommand(async () => await InlineTestSkillAsync(), () => !IsBusy);
            StartAutomationCommand = new RelayCommand(async () => await StartAutomationAsync(), () => !IsBusy && !AutomationRunning);
            StopAutomationCommand = new RelayCommand(() => { AutomationRunning = false; return Task.CompletedTask; }, () => AutomationRunning);

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
            ? "Select a skill to view details."
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

        public StudioPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    OnPropertyChanged(nameof(SelectedPage));
                    if (value == StudioPage.Library) RefreshLibraryAsync().SafeFireAndForget("RefreshLibrary");
                }
            }
        }

        public List<LibrarySkillEntry> LibraryEntries
        {
            get => _libraryEntries;
            set => SetProperty(ref _libraryEntries, value);
        }

        public List<LibrarySkillEntry> FilteredLibraryEntries
        {
            get => _filteredLibraryEntries;
            set => SetProperty(ref _filteredLibraryEntries, value);
        }

        public List<LibraryCategory> LibraryCategories
        {
            get => _libraryCategories;
            set => SetProperty(ref _libraryCategories, value);
        }

        public TargetLibrary SelectedLibraryTier
        {
            get => _selectedLibraryTier;
            set
            {
                if (SetProperty(ref _selectedLibraryTier, value))
                    FilterLibrary();
            }
        }

        public string SelectedLibraryCategory
        {
            get => _selectedLibraryCategory;
            set
            {
                if (SetProperty(ref _selectedLibraryCategory, value))
                    FilterLibrary();
            }
        }

        public string LibrarySearchText
        {
            get => _librarySearchText;
            set
            {
                if (SetProperty(ref _librarySearchText, value))
                    FilterLibrary();
            }
        }

        public LibrarySkillEntry? SelectedLibraryEntry
        {
            get => _selectedLibraryEntry;
            set
            {
                if (SetProperty(ref _selectedLibraryEntry, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                    if (value != null) LoadSkillContent(value);
                }
            }
        }

        public int LibraryUnverifiedCount
        {
            get => _libraryUnverifiedCount;
            set => SetProperty(ref _libraryUnverifiedCount, value);
        }

        public int LibraryTestingCount
        {
            get => _libraryTestingCount;
            set => SetProperty(ref _libraryTestingCount, value);
        }

        public int LibraryVerifiedCount
        {
            get => _libraryVerifiedCount;
            set => SetProperty(ref _libraryVerifiedCount, value);
        }

        public string SelectedLibrarySkillContent
        {
            get => _selectedLibrarySkillContent;
            set => SetProperty(ref _selectedLibrarySkillContent, value);
        }

        public string ImportPath
        {
            get => _importPath;
            set
            {
                if (SetProperty(ref _importPath, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ImportStatus
        {
            get => _importStatus;
            set => SetProperty(ref _importStatus, value);
        }

        public string CreateSkillName
        {
            get => _createSkillName;
            set => SetProperty(ref _createSkillName, value);
        }

        public string CreateSkillDescription
        {
            get => _createSkillDescription;
            set
            {
                if (SetProperty(ref _createSkillDescription, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ImproveSkillGoal
        {
            get => _improveSkillGoal;
            set => SetProperty(ref _improveSkillGoal, value);
        }

        public SkillInfo? ImproveSelectedSkill
        {
            get => _improveSelectedSkill;
            set
            {
                if (SetProperty(ref _improveSelectedSkill, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string TestStatus
        {
            get => _testStatus;
            set => SetProperty(ref _testStatus, value);
        }

        public int AutomationThreshold
        {
            get => _automationThreshold;
            set => SetProperty(ref _automationThreshold, value);
        }

        public int AutomationMaxIterations
        {
            get => _automationMaxIterations;
            set => SetProperty(ref _automationMaxIterations, value);
        }

        public bool AutomationRunning
        {
            get => _automationRunning;
            set
            {
                if (SetProperty(ref _automationRunning, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string AutomationStatus
        {
            get => _automationStatus;
            set => SetProperty(ref _automationStatus, value);
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
        public ICommand NavigateCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand RefreshLibraryCommand { get; }
        public ICommand PromoteSkillCommand { get; }
        public ICommand DemoteSkillCommand { get; }
        public ICommand ImportFromFolderCommand { get; }
        public ICommand InlineCreateSkillCommand { get; }
        public ICommand InlineImproveSkillCommand { get; }
        public ICommand InlineTestSkillCommand { get; }
        public ICommand StartAutomationCommand { get; }
        public ICommand StopAutomationCommand { get; }

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
                AppendOutput($"Loaded {AvailableSkills.Count} skills.");
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
                    Description = $"{AvailableSkills.Count} skills available.",
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

        #region Library Methods

        private async Task RefreshLibraryAsync()
        {
            await Task.Run(() =>
            {
                var repoRoot = ResolveRepoRoot();
                var entries = new List<LibrarySkillEntry>();

                var tiers = new[]
                {
                    (TargetLibrary.LibraryUnverified, "LibraryUnverified"),
                    (TargetLibrary.LibraryWorkbench, "LibraryWorkbench"),
                    (TargetLibrary.Library, "Library"),
                };

                foreach (var (tier, dirName) in tiers)
                {
                    var tierPath = Path.Combine(repoRoot, dirName);
                    if (!Directory.Exists(tierPath)) continue;

                    foreach (var catDir in Directory.GetDirectories(tierPath))
                    {
                        var catName = Path.GetFileName(catDir);
                        if (catName.StartsWith('.')) continue;
                        var catDisplay = FormatCategoryName(catName);

                        foreach (var skillDir in Directory.GetDirectories(catDir))
                        {
                            var skillName = Path.GetFileName(skillDir);
                            if (skillName.StartsWith('.')) continue;
                            var skillMdPath = Path.Combine(skillDir, "SKILL.md");
                            var hasSkillMd = File.Exists(skillMdPath);
                            string? desc = null;
                            if (hasSkillMd)
                            {
                                try
                                {
                                    var lines = File.ReadLines(skillMdPath).Take(10);
                                    desc = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#') && !l.StartsWith("---"));
                                }
                                catch { /* ignore read errors */ }
                            }

                            entries.Add(new LibrarySkillEntry
                            {
                                Name = skillName,
                                DisplayName = FormatSkillName(skillName),
                                Category = catName,
                                CategoryDisplay = catDisplay,
                                FullPath = skillDir,
                                Tier = tier,
                                HasSkillMd = hasSkillMd,
                                Description = desc
                            });
                        }

                        // Also check for direct SKILL.md in category (flat skills)
                        if (File.Exists(Path.Combine(catDir, "SKILL.md")) && !entries.Any(e => e.FullPath == catDir))
                        {
                            entries.Add(new LibrarySkillEntry
                            {
                                Name = catName,
                                DisplayName = FormatSkillName(catName),
                                Category = Path.GetFileName(tierPath),
                                CategoryDisplay = FormatCategoryName(Path.GetFileName(tierPath)),
                                FullPath = catDir,
                                Tier = tier,
                                HasSkillMd = true,
                            });
                        }
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LibraryEntries = entries;
                    LibraryUnverifiedCount = entries.Count(e => e.Tier == TargetLibrary.LibraryUnverified);
                    LibraryTestingCount = entries.Count(e => e.Tier == TargetLibrary.LibraryWorkbench);
                    LibraryVerifiedCount = entries.Count(e => e.Tier == TargetLibrary.Library);
                    LibrarySkillCount = entries.Count;
                    FilterLibrary();
                });
            });
        }

        private void FilterLibrary()
        {
            var filtered = LibraryEntries.Where(e => e.Tier == SelectedLibraryTier);

            if (!string.IsNullOrWhiteSpace(SelectedLibraryCategory))
                filtered = filtered.Where(e => e.Category == SelectedLibraryCategory);

            if (!string.IsNullOrWhiteSpace(LibrarySearchText))
            {
                var search = LibrarySearchText.ToLowerInvariant();
                filtered = filtered.Where(e =>
                    e.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    e.CategoryDisplay.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            FilteredLibraryEntries = filtered.OrderBy(e => e.CategoryDisplay).ThenBy(e => e.DisplayName).ToList();

            // Update categories for the selected tier
            LibraryCategories = LibraryEntries
                .Where(e => e.Tier == SelectedLibraryTier)
                .GroupBy(e => e.Category)
                .Select(g => new LibraryCategory
                {
                    Name = g.Key,
                    DisplayName = FormatCategoryName(g.Key),
                    SkillCount = g.Count(),
                    Tier = SelectedLibraryTier,
                })
                .OrderBy(c => c.DisplayName)
                .ToList();
        }

        private void LoadSkillContent(LibrarySkillEntry entry)
        {
            try
            {
                var skillMdPath = Path.Combine(entry.FullPath, "SKILL.md");
                if (File.Exists(skillMdPath))
                    SelectedLibrarySkillContent = File.ReadAllText(skillMdPath);
                else
                    SelectedLibrarySkillContent = "No SKILL.md file found.";
            }
            catch (Exception ex)
            {
                SelectedLibrarySkillContent = $"Error reading skill: {ex.Message}";
            }
        }

        private async Task PromoteSkillAsync()
        {
            if (SelectedLibraryEntry == null) return;
            var entry = SelectedLibraryEntry;
            var nextTier = entry.Tier switch
            {
                TargetLibrary.LibraryUnverified => "LibraryWorkbench",
                TargetLibrary.LibraryWorkbench => "Library",
                _ => null
            };

            if (nextTier == null)
            {
                AppendOutput("Skill is already in the verified library.");
                return;
            }

            await ExecuteOperationAsync($"Promoting {entry.DisplayName}...", async () =>
            {
                var repoRoot = ResolveRepoRoot();
                var targetDir = Path.Combine(repoRoot, nextTier, entry.Category);
                var targetPath = Path.Combine(targetDir, entry.Name);
                var tempPath = targetPath + "_promoting";

                try
                {
                    Directory.CreateDirectory(targetDir);
                    CopyDirectory(entry.FullPath, tempPath);
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.Move(tempPath, targetPath);
                    Directory.Delete(entry.FullPath, true);
                    AppendOutput($"Promoted {entry.DisplayName} to {nextTier}.");
                    await RefreshLibraryAsync();
                }
                catch
                {
                    if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                    throw;
                }
            });
        }

        private async Task DemoteSkillAsync()
        {
            if (SelectedLibraryEntry == null) return;
            var entry = SelectedLibraryEntry;
            var prevTier = entry.Tier switch
            {
                TargetLibrary.Library => "LibraryWorkbench",
                TargetLibrary.LibraryWorkbench => "LibraryUnverified",
                _ => null
            };

            if (prevTier == null)
            {
                AppendOutput("Skill is already in the unverified library.");
                return;
            }

            await ExecuteOperationAsync($"Demoting {entry.DisplayName}...", async () =>
            {
                var repoRoot = ResolveRepoRoot();
                var targetDir = Path.Combine(repoRoot, prevTier, entry.Category);
                var targetPath = Path.Combine(targetDir, entry.Name);
                var tempPath = targetPath + "_demoting";

                try
                {
                    Directory.CreateDirectory(targetDir);
                    CopyDirectory(entry.FullPath, tempPath);
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    Directory.Move(tempPath, targetPath);
                    Directory.Delete(entry.FullPath, true);
                    AppendOutput($"Demoted {entry.DisplayName} to {prevTier}.");
                    await RefreshLibraryAsync();
                }
                catch
                {
                    if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                    throw;
                }
            });
        }

        private static void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)));
            foreach (var dir in Directory.GetDirectories(src))
                CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }

        private async Task ImportFromFolderAsync()
        {
            if (string.IsNullOrWhiteSpace(ImportPath)) return;
            await ExecuteOperationAsync("Importing skill...", async () =>
            {
                var sourcePath = ImportPath.Trim();
                if (!Directory.Exists(sourcePath))
                {
                    ImportStatus = "Error: Directory does not exist.";
                    return;
                }

                if (!File.Exists(Path.Combine(sourcePath, "SKILL.md")))
                {
                    ImportStatus = "Error: No SKILL.md found in the folder.";
                    return;
                }

                var repoRoot = ResolveRepoRoot();
                var skillName = Path.GetFileName(sourcePath);
                var targetPath = Path.Combine(repoRoot, "LibraryUnverified", "imported", skillName);
                Directory.CreateDirectory(Path.Combine(repoRoot, "LibraryUnverified", "imported"));
                CopyDirectory(sourcePath, targetPath);
                ImportStatus = $"Imported {skillName} to Unverified library.";
                ImportPath = string.Empty;
                await RefreshLibraryAsync();
            });
        }

        private async Task InlineCreateSkillAsync()
        {
            if (string.IsNullOrWhiteSpace(CreateSkillDescription)) return;
            var brief = string.IsNullOrWhiteSpace(CreateSkillName)
                ? CreateSkillDescription
                : $"{CreateSkillName}: {CreateSkillDescription}";

            await ExecuteOperationAsync("Creating skill...", async () =>
            {
                var result = await _pythonService.ExecuteCommandAsync("create", brief);
                AppendOutput(result.Stdout);
                if (result.IsSuccess)
                {
                    AppendOutput("Skill created successfully.");
                    CreateSkillName = string.Empty;
                    CreateSkillDescription = string.Empty;
                    await RefreshLibraryAsync();
                }
            });
        }

        private async Task InlineImproveSkillAsync()
        {
            if (ImproveSelectedSkill == null) return;
            var parameter = string.IsNullOrWhiteSpace(ImproveSkillGoal)
                ? ImproveSelectedSkill.Name
                : $"{ImproveSelectedSkill.Name}|{ImproveSkillGoal}";

            await ExecuteOperationAsync("Improving skill...", async () =>
            {
                var result = await _pythonService.ExecuteCommandAsync("improve", parameter);
                AppendOutput(result.Stdout);
                if (result.IsSuccess)
                {
                    AppendOutput($"Skill '{ImproveSelectedSkill.DisplayName}' improved.");
                    ImproveSkillGoal = string.Empty;
                    await RefreshLibraryAsync();
                }
            });
        }

        private async Task InlineTestSkillAsync()
        {
            var selectedSkill = SelectedLibraryEntry?.Name ?? SelectedLibrarySkill?.Name;
            if (string.IsNullOrEmpty(selectedSkill))
            {
                TestStatus = "Select a skill to test first.";
                return;
            }

            await ExecuteOperationAsync($"Testing {selectedSkill}...", async () =>
            {
                var result = await _pythonService.ExecuteCommandAsync("test", selectedSkill);
                TestStatus = result.IsSuccess ? "Test completed successfully." : "Test failed. See output for details.";
                AppendOutput(result.Stdout);
            });
        }

        private async Task StartAutomationAsync()
        {
            AutomationRunning = true;
            AutomationStatus = "Starting automation loop...";
            var skills = FilteredLibraryEntries.Where(e => e.Tier == TargetLibrary.LibraryWorkbench).Take(10).ToList();

            if (!skills.Any())
            {
                AutomationStatus = "No skills in testing tier. Promote skills first.";
                AutomationRunning = false;
                return;
            }

            for (int iteration = 1; iteration <= AutomationMaxIterations && AutomationRunning; iteration++)
            {
                foreach (var skill in skills)
                {
                    if (!AutomationRunning) break;
                    AutomationStatus = $"Iteration {iteration}/{AutomationMaxIterations}: Testing {skill.DisplayName}...";

                    try
                    {
                        var testResult = await _pythonService.ExecuteCommandAsync("test", skill.Name);
                        AppendOutput($"[Auto] {skill.DisplayName}: {testResult.StatusText}");

                        if (!testResult.IsSuccess && AutomationRunning)
                        {
                            AutomationStatus = $"Iteration {iteration}: Improving {skill.DisplayName}...";
                            await _pythonService.ExecuteCommandAsync("improve", skill.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendOutput($"[Auto] Error on {skill.DisplayName}: {ex.Message}");
                    }
                }
            }

            AutomationStatus = AutomationRunning ? "Automation complete." : "Automation stopped.";
            AutomationRunning = false;
            await RefreshLibraryAsync();
        }

        private static string FormatCategoryName(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return folderName;
            // Strip leading numeric prefix like "01-"
            var stripped = System.Text.RegularExpressions.Regex.Replace(folderName, @"^\d+[-_]?", "");
            if (string.IsNullOrEmpty(stripped)) stripped = folderName;
            return FormatSkillName(stripped);
        }

        private static string FormatSkillName(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return folderName;
            var words = folderName.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i][1..];
            }
            return string.Join(' ', words);
        }

        #endregion

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
