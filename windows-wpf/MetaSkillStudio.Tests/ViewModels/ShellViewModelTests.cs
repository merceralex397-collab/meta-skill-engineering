using System.Collections.Generic;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.Tests.Mocks;
using MetaSkillStudio.ViewModels;
using Xunit;

namespace MetaSkillStudio.Tests.ViewModels
{
    [Collection("WPF isolation")]
    public class ShellViewModelTests
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LibraryPageViewModel _libraryPageViewModel;
        private readonly ImportPageViewModel _importPageViewModel;
        private readonly SettingsPageViewModel _settingsPageViewModel;
        private readonly AnalyticsPageViewModel _analyticsPageViewModel;
        private readonly ShellViewModel _shellViewModel;

        public ShellViewModelTests()
        {
            _mainViewModel = new MainViewModel(new MockPythonRuntimeService(), new MockDialogService(), initializeOnConstruction: false);
            _libraryPageViewModel = new LibraryPageViewModel(_mainViewModel);
            _importPageViewModel = new ImportPageViewModel(_mainViewModel);
            _settingsPageViewModel = new SettingsPageViewModel(_mainViewModel);
            _analyticsPageViewModel = new AnalyticsPageViewModel(_mainViewModel);
            _shellViewModel = new ShellViewModel(
                _mainViewModel,
                _libraryPageViewModel,
                _importPageViewModel,
                _settingsPageViewModel,
                _analyticsPageViewModel);
        }

        [Fact]
        public void Constructor_ComposesShellAndPageViewModels()
        {
            _shellViewModel.LibraryPage.Should().BeSameAs(_libraryPageViewModel);
            _shellViewModel.ImportPage.Should().BeSameAs(_importPageViewModel);
            _shellViewModel.SettingsPage.Should().BeSameAs(_settingsPageViewModel);
            _shellViewModel.AnalyticsPage.Should().BeSameAs(_analyticsPageViewModel);
        }

        [Fact]
        public void ShellViewModel_UsesCoordinatorForNavigationAndShellState()
        {
            var changedProperties = new List<string>();
            _shellViewModel.PropertyChanged += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.PropertyName))
                {
                    changedProperties.Add(e.PropertyName!);
                }
            };

            _shellViewModel.SelectedPage = StudioPage.Analytics;
            _mainViewModel.StatusText = "Refreshing analytics...";

            _mainViewModel.SelectedPage.Should().Be(StudioPage.Analytics);
            _shellViewModel.SelectedPage.Should().Be(StudioPage.Analytics);
            _shellViewModel.StatusText.Should().Be("Refreshing analytics...");
            changedProperties.Should().Contain(nameof(ShellViewModel.SelectedPage));
            changedProperties.Should().Contain(nameof(ShellViewModel.StatusText));
        }

        [Fact]
        public void LibraryPageViewModel_UpdatesCoordinatorSelections()
        {
            _shellViewModel.LibraryPage.SelectedLibraryTier = TargetLibrary.LibraryWorkbench;
            _shellViewModel.LibraryPage.LibrarySearchText = "assistant";

            _mainViewModel.SelectedLibraryTier.Should().Be(TargetLibrary.LibraryWorkbench);
            _mainViewModel.LibrarySearchText.Should().Be("assistant");
            _shellViewModel.LibraryPage.RefreshLibraryCommand.Should().BeSameAs(_mainViewModel.RefreshLibraryCommand);
        }

        [Fact]
        public void ImportPageViewModel_ForwardsImportState()
        {
            var changedProperties = new List<string>();
            _shellViewModel.ImportPage.PropertyChanged += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.PropertyName))
                {
                    changedProperties.Add(e.PropertyName!);
                }
            };

            _shellViewModel.ImportPage.ImportGitHubUrl = "https://github.com/example/demo-skill";
            _mainViewModel.ImportStatus = "Imported successfully.";

            _mainViewModel.ImportGitHubUrl.Should().Be("https://github.com/example/demo-skill");
            _shellViewModel.ImportPage.ImportStatus.Should().Be("Imported successfully.");
            changedProperties.Should().Contain(nameof(ImportPageViewModel.ImportStatus));
        }

        [Fact]
        public void SettingsAndAnalyticsPages_ReflectCoordinatorSummaries()
        {
            _mainViewModel.ProviderStatuses = new List<ProviderStatusInfo>
            {
                new() { Name = "provider-a", Authenticated = true, ModelCount = 2, Raw = "connected" },
                new() { Name = "provider-b", Authenticated = false, ModelCount = 1, Raw = "available" },
            };
            _mainViewModel.RuntimeModels = new List<RuntimeModelInfo>
            {
                new() { Model = "alpha", Recommended = true, Provider = "provider-a" },
                new() { Model = "beta", Recommended = false, Provider = "provider-b" },
            };
            _mainViewModel.AnalyticsMetrics = new List<AnalyticsMetricInfo>
            {
                new() { Label = "Recorded runs", Value = "3" }
            };

            _shellViewModel.SettingsPage.ProviderStatusSummary.Should().Be("1 connected / 2 detected");
            _shellViewModel.AnalyticsPage.AuthenticatedProviderCount.Should().Be(1);
            _shellViewModel.AnalyticsPage.RecommendedModelCount.Should().Be(1);
            _shellViewModel.AnalyticsPage.AnalyticsMetrics.Should().ContainSingle(metric => metric.Label == "Recorded runs");
        }
    }
}
