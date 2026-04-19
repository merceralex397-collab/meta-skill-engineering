using System;
using System.Diagnostics;
using System.Windows;
using MetaSkillStudio.Extensions;
using MetaSkillStudio.Services.Interfaces;
using MetaSkillStudio.ViewModels;

using MessageBox = System.Windows.MessageBox;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Settings dialog for configuring runtime and model assignments per role.
    /// Uses dependency injection for services. View logic only - data context
    /// is provided by SettingsViewModel.
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private readonly SettingsViewModel _viewModel;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public SettingsDialog(IPythonRuntimeService pythonService)
        {
            if (pythonService == null)
                throw new ArgumentNullException(nameof(pythonService));

            _viewModel = new SettingsViewModel(pythonService);
            DataContext = _viewModel;

            InitializeComponent();

            // Bind ViewModel status message to UI element
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SettingsViewModel.RuntimeStatusMessage))
                {
                    RuntimeStatusText.Text = _viewModel.RuntimeStatusMessage;
                }
            };

            // Load existing configuration - SECURITY FIX: Use SafeFireAndForget
            _viewModel.LoadConfigurationAsync().SafeFireAndForget("SettingsDialog.LoadConfigurationAsync");
        }

        // SECURITY FIX: async void event handler with try-catch to prevent application crashes
        // NEVER let exceptions escape from async void methods
        private async void DetectRuntimesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.DetectRuntimesCommand.Execute(null);
            }
            catch (Exception ex)
            {
                // Route error to user and log
                MessageBox.Show($"Error: {ex.Message}", "Detection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"[SettingsDialog] DetectRuntimesButton_Click error: {ex}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SaveConfiguration();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
