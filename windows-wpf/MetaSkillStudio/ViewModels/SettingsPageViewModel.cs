using System.Collections.Generic;
using System.Windows.Input;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.ViewModels
{
    public class SettingsPageViewModel : MainViewModelSectionBase
    {
        public SettingsPageViewModel(MainViewModel coordinator)
            : base(coordinator)
        {
        }

        public string RuntimeStatus => Coordinator.RuntimeStatus;

        public string ProviderStatusSummary => Coordinator.ProviderStatusSummary;

        public List<ProviderStatusInfo> ProviderStatuses => Coordinator.ProviderStatuses;

        public ProviderStatusInfo? SelectedProviderStatus
        {
            get => Coordinator.SelectedProviderStatus;
            set => Coordinator.SelectedProviderStatus = value;
        }

        public List<RuntimeModelInfo> RuntimeModels => Coordinator.RuntimeModels;

        public List<HelpResourceInfo> HelpResources => Coordinator.HelpResources;

        public ICommand RefreshSettingsDataCommand => Coordinator.RefreshSettingsDataCommand;

        public ICommand AddProviderCommand => Coordinator.AddProviderCommand;

        public ICommand SignOutProviderCommand => Coordinator.SignOutProviderCommand;

        protected override void OnCoordinatorPropertyChanged(string? propertyName)
        {
            Forward(
                propertyName,
                nameof(RuntimeStatus),
                nameof(ProviderStatuses),
                nameof(SelectedProviderStatus),
                nameof(RuntimeModels),
                nameof(HelpResources));

            if (string.IsNullOrEmpty(propertyName) ||
                propertyName == nameof(MainViewModel.ProviderStatuses) ||
                propertyName == nameof(MainViewModel.ProviderStatusSummary))
            {
                RaisePropertyChanged(nameof(ProviderStatusSummary));
            }
        }
    }
}
