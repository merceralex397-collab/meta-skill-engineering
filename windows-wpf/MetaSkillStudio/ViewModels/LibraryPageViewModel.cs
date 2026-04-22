using System.Collections.Generic;
using System.Windows.Input;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.ViewModels
{
    public class LibraryPageViewModel : MainViewModelSectionBase
    {
        public LibraryPageViewModel(MainViewModel coordinator)
            : base(coordinator)
        {
        }

        public List<LibrarySkillEntry> LibraryEntries => Coordinator.LibraryEntries;

        public List<LibrarySkillEntry> FilteredLibraryEntries => Coordinator.FilteredLibraryEntries;

        public List<LibraryCategory> LibraryCategories => Coordinator.LibraryCategories;

        public TargetLibrary SelectedLibraryTier
        {
            get => Coordinator.SelectedLibraryTier;
            set => Coordinator.SelectedLibraryTier = value;
        }

        public string SelectedLibraryCategory
        {
            get => Coordinator.SelectedLibraryCategory;
            set => Coordinator.SelectedLibraryCategory = value;
        }

        public string LibrarySearchText
        {
            get => Coordinator.LibrarySearchText;
            set => Coordinator.LibrarySearchText = value;
        }

        public LibrarySkillEntry? SelectedLibraryEntry
        {
            get => Coordinator.SelectedLibraryEntry;
            set => Coordinator.SelectedLibraryEntry = value;
        }

        public int LibraryUnverifiedCount => Coordinator.LibraryUnverifiedCount;

        public int LibraryTestingCount => Coordinator.LibraryTestingCount;

        public int LibraryVerifiedCount => Coordinator.LibraryVerifiedCount;

        public string SelectedLibrarySkillContent => Coordinator.SelectedLibrarySkillContent;

        public ICommand RefreshLibraryCommand => Coordinator.RefreshLibraryCommand;

        public ICommand PromoteSkillCommand => Coordinator.PromoteSkillCommand;

        public ICommand DemoteSkillCommand => Coordinator.DemoteSkillCommand;

        public ICommand MoveSkillCommand => Coordinator.MoveSkillCommand;

        public ICommand InlineTestSkillCommand => Coordinator.InlineTestSkillCommand;

        public ICommand CreateBenchmarksCommand => Coordinator.CreateBenchmarksCommand;

        public ICommand MetaManageCommand => Coordinator.MetaManageCommand;

        protected override void OnCoordinatorPropertyChanged(string? propertyName)
        {
            Forward(
                propertyName,
                nameof(LibraryEntries),
                nameof(FilteredLibraryEntries),
                nameof(LibraryCategories),
                nameof(SelectedLibraryTier),
                nameof(SelectedLibraryCategory),
                nameof(LibrarySearchText),
                nameof(SelectedLibraryEntry),
                nameof(LibraryUnverifiedCount),
                nameof(LibraryTestingCount),
                nameof(LibraryVerifiedCount),
                nameof(SelectedLibrarySkillContent));
        }
    }
}
