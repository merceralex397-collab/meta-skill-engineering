using System.Collections.Generic;
using System.Windows.Input;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.ViewModels
{
    public class AnalyticsPageViewModel : MainViewModelSectionBase
    {
        public AnalyticsPageViewModel(MainViewModel coordinator)
            : base(coordinator)
        {
        }

        public int LibrarySkillCount => Coordinator.LibrarySkillCount;

        public List<RunInfo> RunHistory => Coordinator.RunHistory;

        public int AuthenticatedProviderCount => Coordinator.AuthenticatedProviderCount;

        public int RecommendedModelCount => Coordinator.RecommendedModelCount;

        public List<AnalyticsMetricInfo> AnalyticsMetrics => Coordinator.AnalyticsMetrics;

        public ICommand RefreshAnalyticsCommand => Coordinator.RefreshAnalyticsCommand;

        protected override void OnCoordinatorPropertyChanged(string? propertyName)
        {
            Forward(
                propertyName,
                nameof(LibrarySkillCount),
                nameof(RunHistory),
                nameof(AnalyticsMetrics));

            if (string.IsNullOrEmpty(propertyName) ||
                propertyName == nameof(MainViewModel.ProviderStatuses) ||
                propertyName == nameof(MainViewModel.AuthenticatedProviderCount))
            {
                RaisePropertyChanged(nameof(AuthenticatedProviderCount));
            }

            if (string.IsNullOrEmpty(propertyName) ||
                propertyName == nameof(MainViewModel.RuntimeModels) ||
                propertyName == nameof(MainViewModel.RecommendedModelCount))
            {
                RaisePropertyChanged(nameof(RecommendedModelCount));
            }
        }
    }
}
