using System;
using System.Windows;
using MetaSkillStudio.Models;

using MessageBox = System.Windows.MessageBox;
using MetaSkillStudio.Services.Interfaces;
using MetaSkillStudio.ViewModels;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Dialog for displaying formatted run details with collapsible sections and judge results.
    /// Uses ViewModel for all business logic and data binding.
    /// </summary>
    public partial class RunDetailsDialog : Window
    {
        private readonly RunDetailsViewModel _viewModel;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="pythonService">The Python runtime service.</param>
        /// <param name="runInfo">The run information to display.</param>
        /// <exception cref="ArgumentNullException">Thrown when pythonService or runInfo is null.</exception>
        public RunDetailsDialog(IPythonRuntimeService pythonService, RunInfo runInfo)
        {
            if (pythonService == null) throw new ArgumentNullException(nameof(pythonService));
            if (runInfo == null) throw new ArgumentNullException(nameof(runInfo));

            InitializeComponent();

            _viewModel = new RunDetailsViewModel(runInfo, pythonService);
            DataContext = _viewModel;
        }

        /// <summary>
        /// Handles the Copy button click - copies run details to clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.CopyToClipboard();
                MessageBox.Show("Run details copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Close button click - closes the dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
