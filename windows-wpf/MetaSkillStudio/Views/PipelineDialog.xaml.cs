using System;
using System.Windows;
using System.Windows.Controls;
using MetaSkillStudio.Services.Interfaces;
using MetaSkillStudio.ViewModels;

using MessageBox = System.Windows.MessageBox;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Pipeline execution dialog with progress tracking.
    /// Uses PipelineViewModel for all business logic and state management.
    /// </summary>
    public partial class PipelineDialog : Window
    {
        private readonly PipelineViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the PipelineDialog class.
        /// </summary>
        /// <param name="pythonService">The Python runtime service for pipeline execution.</param>
        public PipelineDialog(IPythonRuntimeService pythonService)
        {
            _viewModel = new PipelineViewModel(pythonService);
            DataContext = _viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Handles the New Skill checkbox checked event.
        /// Disables the target skill text box when creating a new skill.
        /// </summary>
        private void NewSkillCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TargetSkillTextBox.IsEnabled = false;
            TargetSkillTextBox.Clear();
        }

        /// <summary>
        /// Handles the New Skill checkbox unchecked event.
        /// Enables the target skill text box for existing skill selection.
        /// </summary>
        private void NewSkillCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TargetSkillTextBox.IsEnabled = true;
        }

        /// <summary>
        /// Handles the Start button click event.
        /// SECURITY FIX: async void event handler with try-catch to prevent application crashes.
        /// NEVER let exceptions escape from async void methods.
        /// </summary>
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsRunning) return;

            // Update UI state before starting
            StartButton.IsEnabled = false;
            StatusMessage.Text = "Starting pipeline...";
            OverallProgress.Visibility = Visibility.Visible;
            BusyOverlay.Visibility = Visibility.Visible;

            try
            {
                // Get configuration from UI controls
                var pipelineType = ((ComboBoxItem)PipelineTypeCombo.SelectedItem).Tag?.ToString() ?? "creation";
                var targetSkill = NewSkillCheckBox.IsChecked == true ? null : TargetSkillTextBox.Text.Trim();
                var brief = BriefTextBox.Text.Trim();

                // Execute pipeline through ViewModel
                await _viewModel.StartPipelineAsync(pipelineType, targetSkill, brief);

                // Update status from ViewModel
                StatusMessage.Text = _viewModel.StatusMessage;
                OverallProgress.Value = _viewModel.OverallProgress;
            }
            catch (Exception ex)
            {
                StatusMessage.Text = $"Error: {ex.Message}";
            }
            finally
            {
                StartButton.IsEnabled = true;
                StartButton.Content = "Start Pipeline";
                BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Cancel button click event.
        /// Closes the dialog, optionally showing a message if pipeline is still running.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsRunning)
            {
                MessageBox.Show(
                    "Pipeline will continue running in background.",
                    "Note",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            DialogResult = false;
            Close();
        }
    }
}
