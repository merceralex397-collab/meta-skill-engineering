using System.Collections.ObjectModel;
using System.Windows.Input;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.ViewModels
{
    public class ImportPageViewModel : MainViewModelSectionBase
    {
        public ImportPageViewModel(MainViewModel coordinator)
            : base(coordinator)
        {
        }

        public ObservableCollection<TargetLibrary> ImportLibraryOptions => Coordinator.ImportLibraryOptions;

        public string ImportPath
        {
            get => Coordinator.ImportPath;
            set => Coordinator.ImportPath = value;
        }

        public string ImportGitHubUrl
        {
            get => Coordinator.ImportGitHubUrl;
            set => Coordinator.ImportGitHubUrl = value;
        }

        public string ImportStatus => Coordinator.ImportStatus;

        public string ImportCategory
        {
            get => Coordinator.ImportCategory;
            set => Coordinator.ImportCategory = value;
        }

        public TargetLibrary SelectedImportLibrary
        {
            get => Coordinator.SelectedImportLibrary;
            set => Coordinator.SelectedImportLibrary = value;
        }

        public ICommand ImportFromFolderCommand => Coordinator.ImportFromFolderCommand;

        public ICommand ImportFromGitHubCommand => Coordinator.ImportFromGitHubCommand;

        protected override void OnCoordinatorPropertyChanged(string? propertyName)
        {
            Forward(
                propertyName,
                nameof(ImportPath),
                nameof(ImportGitHubUrl),
                nameof(ImportStatus),
                nameof(ImportCategory),
                nameof(SelectedImportLibrary));
        }
    }
}
