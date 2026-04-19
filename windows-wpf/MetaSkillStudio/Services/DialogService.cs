using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MetaSkillStudio.Models;
using MetaSkillStudio.Views;
using MetaSkillStudio.ViewModels;
using MetaSkillStudio.Services.Interfaces;

using MessageBox = System.Windows.MessageBox;

namespace MetaSkillStudio.Services
{
    /// <summary>
    /// Implementation of IDialogService for displaying dialogs and UI interactions.
    /// Uses DI container to resolve dialog dependencies.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the DialogService class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Shows a message box with the specified message and title.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box window.</param>
        /// <param name="type">The type of message (determines the icon). Defaults to Information.</param>
        public void ShowMessage(string message, string title, MessageType type = MessageType.Information)
        {
            var button = MessageBoxButton.OK;
            var image = type switch
            {
                MessageType.Warning => MessageBoxImage.Warning,
                MessageType.Error => MessageBoxImage.Error,
                MessageType.Question => MessageBoxImage.Question,
                _ => MessageBoxImage.Information
            };

            MessageBox.Show(message, title, button, image);
        }

        /// <summary>
        /// Shows a dialog of type T and returns the dialog result.
        /// </summary>
        /// <typeparam name="T">The type of dialog to show. Must be a supported dialog type.</typeparam>
        /// <returns>True if the dialog was accepted, false if cancelled, or null if closed without result.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified dialog type is not supported.</exception>
        public bool? ShowDialog<T>() where T : class
        {
            if (typeof(T) == typeof(CreateSkillDialog))
            {
                var dialog = new CreateSkillDialog();
                return dialog.ShowDialog();
            }

            if (typeof(T) == typeof(SettingsDialog))
            {
                var pythonService = _serviceProvider.GetRequiredService<IPythonRuntimeService>();
                var dialog = new SettingsDialog(pythonService);
                return dialog.ShowDialog();
            }

            if (typeof(T) == typeof(BenchmarkDialog))
            {
                var dialog = new BenchmarkDialog();
                return dialog.ShowDialog();
            }

            if (typeof(T) == typeof(AnalyticsDialog))
            {
                var viewModel = _serviceProvider.GetRequiredService<AnalyticsViewModel>();
                var dialog = new AnalyticsDialog(viewModel);
                return dialog.ShowDialog();
            }

            throw new NotSupportedException($"Dialog type {typeof(T).Name} is not supported.");
        }

        /// <summary>
        /// Shows the create skill dialog and returns the result.
        /// </summary>
        /// <returns>A tuple containing:
        /// - Result: True if dialog was accepted, false if cancelled, or null if closed
        /// - SkillBrief: The brief description entered by the user
        /// - TargetLibrary: The selected target library for the new skill</returns>
        public (bool? Result, string SkillBrief, TargetLibrary TargetLibrary) ShowCreateSkillDialog()
        {
            var dialog = new CreateSkillDialog();
            var result = dialog.ShowDialog();
            return (result, dialog.SkillBrief, dialog.TargetLibrary);
        }

        /// <summary>
        /// Shows the skill selection dialog and returns the selected skill.
        /// </summary>
        /// <param name="skills">The list of available skills to display.</param>
        /// <param name="description">The description text to display in the dialog.</param>
        /// <param name="allowTestAll">Whether to allow the user to select all skills for testing. Defaults to false.</param>
        /// <returns>A tuple containing:
        /// - Result: True if dialog was accepted, false if cancelled, or null if closed
        /// - SelectedSkill: The skill selected by the user, or null if none selected
        /// - TestAllSkills: True if the user chose to test all skills</returns>
        public (bool? Result, SkillInfo? SelectedSkill, bool TestAllSkills) ShowSkillSelectionDialog(
            List<SkillInfo> skills, 
            string description, 
            bool allowTestAll = false)
        {
            var dialog = new SkillSelectionDialog(skills, description, allowTestAll);
            var result = dialog.ShowDialog();
            return (result, dialog.SelectedSkill, dialog.TestAllSkills);
        }

        /// <summary>
        /// Shows an input dialog and returns the user input.
        /// </summary>
        /// <param name="title">The title of the input dialog.</param>
        /// <param name="message">The message to display in the input dialog.</param>
        /// <param name="defaultResponse">The default text to populate in the input field. Defaults to empty string.</param>
        /// <returns>A tuple containing:
        /// - Result: True if dialog was accepted, false if cancelled, or null if closed
        /// - ResponseText: The text entered by the user</returns>
        public (bool? Result, string ResponseText) ShowInputDialog(string title, string message, string defaultResponse = "")
        {
            var dialog = new InputDialog(title, message, defaultResponse);
            var result = dialog.ShowDialog();
            return (result, dialog.ResponseText);
        }

        /// <summary>
        /// Shows the benchmark dialog and returns the configuration.
        /// </summary>
        /// <returns>A tuple containing:
        /// - Result: True if dialog was accepted, false if cancelled, or null if closed
        /// - SkillName: The name of the skill to benchmark
        /// - BenchmarkGoal: The goal or description of the benchmark
        /// - CaseCount: The number of benchmark cases to create</returns>
        public (bool? Result, string SkillName, string BenchmarkGoal, int CaseCount) ShowBenchmarkDialog()
        {
            var dialog = new BenchmarkDialog();
            var result = dialog.ShowDialog();
            var viewModel = dialog.DataContext as BenchmarkViewModel;
            if (viewModel != null && result == true)
            {
                return (result, viewModel.SkillName, viewModel.BenchmarkGoal, viewModel.CaseCount);
            }
            return (result, string.Empty, string.Empty, 0);
        }

        /// <summary>
        /// Shows the settings dialog.
        /// </summary>
        /// <returns>True if settings were saved, false if cancelled, or null if closed without result.</returns>
        public bool? ShowSettingsDialog()
        {
            var pythonService = _serviceProvider.GetRequiredService<IPythonRuntimeService>();
            var dialog = new SettingsDialog(pythonService);
            return dialog.ShowDialog();
        }

        /// <summary>
        /// Shows the analytics dialog for viewing skill performance metrics.
        /// </summary>
        public void ShowAnalyticsDialog()
        {
            var viewModel = _serviceProvider.GetRequiredService<AnalyticsViewModel>();
            var dialog = new AnalyticsDialog(viewModel);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Shows the run details dialog for the specified run.
        /// </summary>
        /// <param name="runInfo">The run information to display details for.</param>
        /// <exception cref="ArgumentNullException">Thrown when runInfo is null.</exception>
        public void ShowRunDetailsDialog(RunInfo runInfo)
        {
            var pythonService = _serviceProvider.GetRequiredService<IPythonRuntimeService>();
            var dialog = new RunDetailsDialog(pythonService, runInfo);
            dialog.ShowDialog();
        }
    }
}
