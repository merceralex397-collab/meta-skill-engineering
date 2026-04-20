using System;
using System.Linq;
using FluentAssertions;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;
using MetaSkillStudio.Tests.Mocks;
using Xunit;

namespace MetaSkillStudio.Tests.Integration
{
    /// <summary>
    /// Integration tests for service interactions and DI configuration.
    /// </summary>
    public class ServiceIntegrationTests
    {
        #region Mock Service Integration

        [Fact]
        public void MockPythonRuntimeService_TracksAllCalls()
        {
            // Arrange
            var mockService = new MockPythonRuntimeService();
            mockService.AddDetectedRuntime(new DetectedRuntime { Name = "test", Command = "test" });

            // Act
            var result1 = mockService.DetectRuntimesAsync().Result;
            var result2 = mockService.DetectRuntimesAsync().Result;

            // Assert
            mockService.DetectRuntimesCallCount.Should().Be(2);
            result1.Should().HaveCount(1);
            result2.Should().HaveCount(1);
        }

        [Fact]
        public void MockPythonRuntimeService_ExecuteCommand_TracksParameters()
        {
            // Arrange
            var mockService = new MockPythonRuntimeService();

            // Act
            mockService.ExecuteCommandAsync("create", "test brief", TargetLibrary.LibraryWorkbench).Wait();
            mockService.ExecuteCommandAsync("test", "test-skill", TargetLibrary.LibraryUnverified).Wait();

            // Assert
            mockService.ExecuteCommandCallCount.Should().Be(2);
            mockService.ExecuteCommandParameters.Should().HaveCount(2);
            mockService.ExecuteCommandParameters[0].Action.Should().Be("create");
            mockService.ExecuteCommandParameters[0].Library.Should().Be(TargetLibrary.LibraryWorkbench);
            mockService.ExecuteCommandParameters[1].Action.Should().Be("test");
        }

        [Fact]
        public void MockDialogService_TracksAllDialogCalls()
        {
            // Arrange
            var mockDialog = new MockDialogService();

            // Act
            mockDialog.ShowMessage("Test", "Title");
            mockDialog.ShowMessage("Test 2", "Title 2");

            // Assert
            mockDialog.MessageBoxCalls.Should().HaveCount(2);
        }

        [Fact]
        public void MockDialogService_ShowCreateSkillDialog_ReturnsConfiguredResult()
        {
            // Arrange
            var mockDialog = new MockDialogService();
            mockDialog.NextCreateSkillDialogResult = (true, "Test Skill", TargetLibrary.LibraryWorkbench);

            // Act
            var result = mockDialog.ShowCreateSkillDialog();

            // Assert
            result.Result.Should().BeTrue();
            result.SkillBrief.Should().Be("Test Skill");
            result.TargetLibrary.Should().Be(TargetLibrary.LibraryWorkbench);
        }

        [Fact]
        public void MockDialogService_ShowSkillSelectionDialog_ReturnsSkillFromQueue()
        {
            // Arrange
            var mockDialog = new MockDialogService();
            var skill1 = new SkillInfo { Name = "skill-1" };
            var skill2 = new SkillInfo { Name = "skill-2" };
            
            mockDialog.SkillSelectionDialogResultsQueue.Add((true, skill1, false));
            mockDialog.SkillSelectionDialogResultsQueue.Add((true, skill2, false));

            var skills = new System.Collections.Generic.List<SkillInfo> { skill1, skill2 };

            // Act
            var result1 = mockDialog.ShowSkillSelectionDialog(skills, "Test");
            var result2 = mockDialog.ShowSkillSelectionDialog(skills, "Test");

            // Assert
            result1.SelectedSkill.Should().Be(skill1);
            result2.SelectedSkill.Should().Be(skill2);
        }

        [Fact]
        public void MockDialogService_Reset_ClearsAllTracking()
        {
            // Arrange
            var mockDialog = new MockDialogService();
            mockDialog.ShowMessage("Test", "Title");
            mockDialog.ShowCreateSkillDialog();

            // Act
            mockDialog.Reset();

            // Assert
            mockDialog.MessageBoxCalls.Should().BeEmpty();
            mockDialog.CreateSkillDialogCallCount.Should().Be(0);
        }

        [Fact]
        public void MockConfigurationStorage_LoadAndSave_WorksCorrectly()
        {
            // Arrange
            var storage = new MockConfigurationStorage();
            var config = new AppConfiguration
            {
                Version = 1,
                DetectedRuntimes = new System.Collections.Generic.List<DetectedRuntime>
                {
                    new DetectedRuntime { Name = "test", Command = "test" }
                }
            };

            // Act
            storage.Save(config);
            var loaded = storage.Load();

            // Assert
            storage.SaveCallCount.Should().Be(1);
            storage.LoadCallCount.Should().Be(1);
            loaded.Should().NotBeNull();
            loaded!.Version.Should().Be(1);
            loaded.DetectedRuntimes.Should().HaveCount(1);
        }

        [Fact]
        public void MockConfigurationStorage_SaveHistory_TracksAllSaves()
        {
            // Arrange
            var storage = new MockConfigurationStorage();
            var config1 = new AppConfiguration { Version = 1 };
            var config2 = new AppConfiguration { Version = 2 };

            // Act
            storage.Save(config1);
            storage.Save(config2);

            // Assert
            storage.SaveHistory.Should().HaveCount(2);
            storage.SaveHistory[0].Version.Should().Be(1);
            storage.SaveHistory[1].Version.Should().Be(2);
        }

        [Fact]
        public void MockDispatcher_InvokesActionsImmediately()
        {
            // Arrange
            var dispatcher = new MockDispatcher();
            var invoked = false;

            // Act
            dispatcher.Invoke(() => invoked = true);

            // Assert
            invoked.Should().BeTrue();
            dispatcher.InvokeCallCount.Should().Be(1);
            dispatcher.InvokedActions.Should().HaveCount(1);
        }

        [Fact]
        public void MockDispatcher_InvokeAsync_RunsSynchronously()
        {
            // Arrange
            var dispatcher = new MockDispatcher();
            var invoked = false;

            // Act
            dispatcher.InvokeAsync(() => { invoked = true; }).Wait();

            // Assert
            invoked.Should().BeTrue();
            dispatcher.InvokeAsyncCallCount.Should().Be(1);
        }

        [Fact]
        public void MockEnvironmentProvider_StoresAndReturnsValues()
        {
            // Arrange
            var provider = new MockEnvironmentProvider();
            provider.SetEnvironmentVariable("TEST_VAR", "test_value");

            // Act
            var result = provider.GetEnvironmentVariable("TEST_VAR");

            // Assert
            result.Should().Be("test_value");
            provider.GetEnvironmentVariableCallCount.Should().Be(1);
        }

        [Fact]
        public void MockEnvironmentProvider_FileExists_UsesConfiguredValues()
        {
            // Arrange
            var provider = new MockEnvironmentProvider();
            provider.SetFileExists("C:\\test.txt", true);
            provider.SetFileExists("C:\\missing.txt", false);

            // Act & Assert
            provider.FileExists("C:\\test.txt").Should().BeTrue();
            provider.FileExists("C:\\missing.txt").Should().BeFalse();
            provider.FileExistsCallCount.Should().Be(2);
        }

        #endregion

        #region Service Interaction Tests

        [Fact]
        public void MultipleServices_CanWorkTogether()
        {
            // Arrange
            var pythonService = new MockPythonRuntimeService();
            var configStorage = new MockConfigurationStorage();
            var dialogService = new MockDialogService();

            // Simulate a workflow:
            // 1. Load configuration
            var config = new AppConfiguration
            {
                DetectedRuntimes = new System.Collections.Generic.List<DetectedRuntime>
                {
                    new DetectedRuntime { Name = "opencode", Command = "opencode" }
                }
            };
            configStorage.Save(config);

            // 2. Use python service with loaded config
            pythonService.SetConfiguration(configStorage.Load());
            var loadedConfig = pythonService.LoadConfiguration();

            // 3. Show dialog with config info
            dialogService.NextSettingsDialogResult = true;
            var result = dialogService.ShowSettingsDialog();

            // Assert
            result.Should().BeTrue();
            loadedConfig.Should().NotBeNull();
            pythonService.LoadConfigurationCallCount.Should().Be(1);
            configStorage.LoadCallCount.Should().Be(1);
            dialogService.SettingsDialogCallCount.Should().Be(1);
        }

        [Fact]
        public void ServiceConfiguration_CanBePassedBetweenServices()
        {
            // Arrange
            var pythonService = new MockPythonRuntimeService();
            var storage = new MockConfigurationStorage();

            // Act - Simulate saving config from python service through storage
            var config = pythonService.CreateDefaultConfigurationAsync().Result;
            storage.Save(config);

            // Assert
            storage.StoredConfiguration.Should().NotBeNull();
            storage.StoredConfiguration!.DetectedRuntimes.Should().BeEquivalentTo(config.DetectedRuntimes);
        }

        #endregion
    }
}
