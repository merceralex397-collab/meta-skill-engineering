using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.ViewModels
{
    public class ShellViewModel : MainViewModelSectionBase
    {
        public ShellViewModel(
            MainViewModel coordinator,
            LibraryPageViewModel libraryPage,
            ImportPageViewModel importPage,
            SettingsPageViewModel settingsPage,
            AnalyticsPageViewModel analyticsPage)
            : base(coordinator)
        {
            LibraryPage = libraryPage;
            ImportPage = importPage;
            SettingsPage = settingsPage;
            AnalyticsPage = analyticsPage;
        }

        public LibraryPageViewModel LibraryPage { get; }

        public ImportPageViewModel ImportPage { get; }

        public SettingsPageViewModel SettingsPage { get; }

        public AnalyticsPageViewModel AnalyticsPage { get; }

        public string OutputText
        {
            get => Coordinator.OutputText;
            set => Coordinator.OutputText = value;
        }

        public string StatusText
        {
            get => Coordinator.StatusText;
            set => Coordinator.StatusText = value;
        }

        public string RuntimeStatusDisplay => Coordinator.RuntimeStatusDisplay;

        public bool IsBusy
        {
            get => Coordinator.IsBusy;
            set => Coordinator.IsBusy = value;
        }

        public bool IsAssistantBusy => Coordinator.IsAssistantBusy;

        public bool IsAssistantPanelOpen
        {
            get => Coordinator.IsAssistantPanelOpen;
            set => Coordinator.IsAssistantPanelOpen = value;
        }

        public string AssistantPrompt
        {
            get => Coordinator.AssistantPrompt;
            set => Coordinator.AssistantPrompt = value;
        }

        public string AssistantStatus => Coordinator.AssistantStatus;

        public string SelectedAssistantModel
        {
            get => Coordinator.SelectedAssistantModel;
            set => Coordinator.SelectedAssistantModel = value;
        }

        public string AssistantActiveModel => Coordinator.AssistantActiveModel;

        public string QuickStartSummary => Coordinator.QuickStartSummary;

        public StudioPage SelectedPage
        {
            get => Coordinator.SelectedPage;
            set => Coordinator.SelectedPage = value;
        }

        public int LibrarySkillCount => Coordinator.LibrarySkillCount;

        public int LibraryUnverifiedCount => Coordinator.LibraryUnverifiedCount;

        public int LibraryTestingCount => Coordinator.LibraryTestingCount;

        public int LibraryVerifiedCount => Coordinator.LibraryVerifiedCount;

        public ObservableCollection<ChatMessage> ChatHistory => Coordinator.ChatHistory;

        public ObservableCollection<string> AssistantModelOptions => Coordinator.AssistantModelOptions;

        public ObservableCollection<TargetLibrary> ImportLibraryOptions => Coordinator.ImportLibraryOptions;

        public List<SkillInfo> AvailableSkills => Coordinator.AvailableSkills;

        public string CreateSkillName
        {
            get => Coordinator.CreateSkillName;
            set => Coordinator.CreateSkillName = value;
        }

        public string CreateSkillDescription
        {
            get => Coordinator.CreateSkillDescription;
            set => Coordinator.CreateSkillDescription = value;
        }

        public TargetLibrary SelectedCreateLibrary
        {
            get => Coordinator.SelectedCreateLibrary;
            set => Coordinator.SelectedCreateLibrary = value;
        }

        public string ImproveSkillGoal
        {
            get => Coordinator.ImproveSkillGoal;
            set => Coordinator.ImproveSkillGoal = value;
        }

        public SkillInfo? ImproveSelectedSkill
        {
            get => Coordinator.ImproveSelectedSkill;
            set => Coordinator.ImproveSelectedSkill = value;
        }

        public string TestStatus => Coordinator.TestStatus;

        public SkillInfo? TestSelectedSkill
        {
            get => Coordinator.TestSelectedSkill;
            set => Coordinator.TestSelectedSkill = value;
        }

        public int AutomationThreshold
        {
            get => Coordinator.AutomationThreshold;
            set => Coordinator.AutomationThreshold = value;
        }

        public int AutomationMaxIterations
        {
            get => Coordinator.AutomationMaxIterations;
            set => Coordinator.AutomationMaxIterations = value;
        }

        public bool AutomationRunning => Coordinator.AutomationRunning;

        public string AutomationStatus => Coordinator.AutomationStatus;

        public ICommand CreateSkillCommand => Coordinator.CreateSkillCommand;

        public ICommand ImproveSkillCommand => Coordinator.ImproveSkillCommand;

        public ICommand TestBenchmarkCommand => Coordinator.TestBenchmarkCommand;

        public ICommand RefreshLibraryCommand => Coordinator.RefreshLibraryCommand;

        public ICommand ToggleAssistantCommand => Coordinator.ToggleAssistantCommand;

        public ICommand OpenQuickStartCommand => Coordinator.OpenQuickStartCommand;

        public ICommand NavigateCommand => Coordinator.NavigateCommand;

        public ICommand ClearChatCommand => Coordinator.ClearChatCommand;

        public ICommand NewConversationCommand => Coordinator.NewConversationCommand;

        public ICommand AssistantPromptCommand => Coordinator.AssistantPromptCommand;

        public ICommand CreateBenchmarksCommand => Coordinator.CreateBenchmarksCommand;

        public ICommand FindExternalSkillsCommand => Coordinator.FindExternalSkillsCommand;

        public ICommand InlineCreateSkillCommand => Coordinator.InlineCreateSkillCommand;

        public ICommand InlineImproveSkillCommand => Coordinator.InlineImproveSkillCommand;

        public ICommand InlineTestSkillCommand => Coordinator.InlineTestSkillCommand;

        public ICommand StartAutomationCommand => Coordinator.StartAutomationCommand;

        public ICommand StopAutomationCommand => Coordinator.StopAutomationCommand;

        protected override void OnCoordinatorPropertyChanged(string? propertyName)
        {
            Forward(
                propertyName,
                nameof(OutputText),
                nameof(StatusText),
                nameof(RuntimeStatusDisplay),
                nameof(IsBusy),
                nameof(IsAssistantBusy),
                nameof(IsAssistantPanelOpen),
                nameof(AssistantPrompt),
                nameof(AssistantStatus),
                nameof(SelectedAssistantModel),
                nameof(AssistantActiveModel),
                nameof(QuickStartSummary),
                nameof(SelectedPage),
                nameof(LibrarySkillCount),
                nameof(LibraryUnverifiedCount),
                nameof(LibraryTestingCount),
                nameof(LibraryVerifiedCount),
                nameof(AvailableSkills),
                nameof(CreateSkillName),
                nameof(CreateSkillDescription),
                nameof(SelectedCreateLibrary),
                nameof(ImproveSkillGoal),
                nameof(ImproveSelectedSkill),
                nameof(TestStatus),
                nameof(TestSelectedSkill),
                nameof(AutomationThreshold),
                nameof(AutomationMaxIterations),
                nameof(AutomationRunning),
                nameof(AutomationStatus));
        }
    }
}
