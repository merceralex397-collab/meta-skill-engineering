using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.ViewModels;
using MetaSkillStudio.Tests.Mocks;
using MetaSkillStudio.Tests.Helpers;
using Xunit;

namespace MetaSkillStudio.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for MainViewModel.
    /// Tests properties, commands, and interactions with services.
    /// </summary>
    public class MainViewModelTests
    {
        private readonly MockPythonRuntimeService _mockPythonService;
        private readonly MockDialogService _mockDialogService;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockPythonService = new MockPythonRuntimeService();
            _mockDialogService = new MockDialogService();
            _viewModel = new MainViewModel(_mockPythonService, _mockDialogService);
        }

        #region Property Tests

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Assert
            _viewModel.OutputText.Should().Contain("Welcome to Meta Skill Studio");
            _viewModel.StatusText.Should().Be("Ready");
            _viewModel.IsBusy.Should().BeFalse();
            _viewModel.RuntimeStatus.Should().NotBeNull();
            _viewModel.AvailableSkills.Should().BeEmpty();
        }

        [Fact]
        public void IsBusy_SetToTrue_DisablesCommands()
        {
            // Arrange
            var canExecuteBefore = _viewModel.CreateSkillCommand.CanExecute(null);

            // Act
            _viewModel.IsBusy = true;

            // Assert
            canExecuteBefore.Should().BeTrue();
            _viewModel.CreateSkillCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void IsBusy_SetToFalse_EnablesCommands()
        {
            // Arrange
            _viewModel.IsBusy = true;

            // Act
            _viewModel.IsBusy = false;

            // Assert
            _viewModel.CreateSkillCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void OutputText_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.OutputText))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.OutputText = "Test output";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.OutputText.Should().Be("Test output");
        }

        [Fact]
        public void StatusText_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.StatusText))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.StatusText = "Working...";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.StatusText.Should().Be("Working...");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void Commands_AreInitialized()
        {
            // Assert
            _viewModel.CreateSkillCommand.Should().NotBeNull();
            _viewModel.ImproveSkillCommand.Should().NotBeNull();
            _viewModel.TestBenchmarkCommand.Should().NotBeNull();
            _viewModel.MetaManageCommand.Should().NotBeNull();
            _viewModel.CreateBenchmarksCommand.Should().NotBeNull();
            _viewModel.RefreshRunsCommand.Should().NotBeNull();
            _viewModel.OpenSettingsCommand.Should().NotBeNull();
            _viewModel.RefreshSkillsCommand.Should().NotBeNull();
            _viewModel.OpenAnalyticsCommand.Should().NotBeNull();
        }

        [Fact]
        public void Commands_CanExecute_WhenNotBusy()
        {
            // Arrange
            _viewModel.IsBusy = false;

            // Assert
            _viewModel.CreateSkillCommand.CanExecute(null).Should().BeTrue();
            _viewModel.ImproveSkillCommand.CanExecute(null).Should().BeTrue();
            _viewModel.TestBenchmarkCommand.CanExecute(null).Should().BeTrue();
            _viewModel.MetaManageCommand.CanExecute(null).Should().BeTrue();
            _viewModel.CreateBenchmarksCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void Commands_CannotExecute_WhenBusy()
        {
            // Arrange
            _viewModel.IsBusy = true;

            // Assert
            _viewModel.CreateSkillCommand.CanExecute(null).Should().BeFalse();
            _viewModel.ImproveSkillCommand.CanExecute(null).Should().BeFalse();
            _viewModel.TestBenchmarkCommand.CanExecute(null).Should().BeFalse();
            _viewModel.MetaManageCommand.CanExecute(null).Should().BeFalse();
            _viewModel.CreateBenchmarksCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Service Integration Tests

        [Fact]
        public async Task RefreshSkillsAsync_CallsListSkillsService()
        {
            // Arrange
            _mockPythonService.AddSkill(TestDataGenerator.CreateSkillInfo("test-skill-1"));
            _mockPythonService.AddSkill(TestDataGenerator.CreateSkillInfo("test-skill-2"));

            // Act
            // Use reflection to call private method
            var method = typeof(MainViewModel).GetMethod("RefreshSkillsAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            _mockPythonService.ListSkillsCallCount.Should().BeGreaterThanOrEqualTo(1);
            _viewModel.AvailableSkills.Count.Should().Be(2);
        }

        [Fact]
        public async Task RefreshSkillsAsync_SetsIsBusyDuringOperation()
        {
            // Arrange
            _mockPythonService.AddSkill(TestDataGenerator.CreateSkillInfo());
            var isBusyStates = new List<bool>();
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsBusy))
                    isBusyStates.Add(_viewModel.IsBusy);
            };

            // Act
            var method = typeof(MainViewModel).GetMethod("RefreshSkillsAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            isBusyStates.Should().Contain(true);
            isBusyStates.Should().Contain(false);
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshSkillsAsync_AppendsOutput()
        {
            // Arrange
            var initialOutput = _viewModel.OutputText;
            _mockPythonService.AddSkill(TestDataGenerator.CreateSkillInfo());

            // Act
            var method = typeof(MainViewModel).GetMethod("RefreshSkillsAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            _viewModel.OutputText.Should().NotBe(initialOutput);
            _viewModel.OutputText.Should().Contain("Loaded");
        }

        [Fact]
        public async Task UpdateRuntimeStatusAsync_UpdatesStatusText()
        {
            // Arrange
            _mockPythonService.AddDetectedRuntime(TestDataGenerator.CreateDetectedRuntime("codex", isAvailable: true));

            // Act
            var method = typeof(MainViewModel).GetMethod("UpdateRuntimeStatusAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            _viewModel.RuntimeStatus.Should().Contain("1 runtime(s) available");
        }

        [Fact]
        public async Task UpdateRuntimeStatusAsync_HandlesDetectionFailure()
        {
            // Arrange
            _mockPythonService.ShouldThrowOnDetectRuntimes = true;

            // Act
            var method = typeof(MainViewModel).GetMethod("UpdateRuntimeStatusAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            _viewModel.RuntimeStatus.Should().Contain("Runtime detection failed");
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ExecuteOperationAsync_SetsErrorStatusOnFailure()
        {
            // Arrange
            _mockPythonService.ShouldThrowOnListSkills = true;

            // Act
            var method = typeof(MainViewModel).GetMethod("RefreshSkillsAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_viewModel, null)!;

            // Assert
            _viewModel.StatusText.Should().Contain("Error");
            _viewModel.OutputText.Should().Contain("ERROR");
        }

        [Fact]
        public async Task ExecuteOperationAsync_ResetsIsBusyAfterError()
        {
            // Arrange
            _mockPythonService.ShouldThrowOnListSkills = true;

            // Act
            try
            {
                var method = typeof(MainViewModel).GetMethod("RefreshSkillsAsync",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                await (Task)method!.Invoke(_viewModel, null)!;
            }
            catch
            {
                // Expected
            }

            // Assert
            _viewModel.IsBusy.Should().BeFalse();
        }

        #endregion

        #region RunHistory Tests

        [Fact]
        public void RunHistory_IsInitialized()
        {
            // Assert
            _viewModel.RunHistory.Should().NotBeNull();
        }

        [Fact]
        public void SelectedRun_SetValue_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedRun))
                    propertyChangedRaised = true;
            };

            var runInfo = TestDataGenerator.CreateRunInfo();

            // Act
            _viewModel.SelectedRun = runInfo;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.SelectedRun.Should().Be(runInfo);
        }

        #endregion
    }
}
