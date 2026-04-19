using System.Windows;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Dialog for creating benchmark cases with configurable count.
    /// Uses BenchmarkViewModel for data binding and command handling.
    /// </summary>
    public partial class BenchmarkDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the BenchmarkDialog class.
        /// </summary>
        public BenchmarkDialog()
        {
            InitializeComponent();
            DataContext = new ViewModels.BenchmarkViewModel();
        }

        /// <summary>
        /// Gets the ViewModel for this dialog.
        /// </summary>
        private ViewModels.BenchmarkViewModel ViewModel => (ViewModels.BenchmarkViewModel)DataContext;

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation is handled by the ViewModel
            if (!ViewModel.IsValid)
            {
                System.Windows.MessageBox.Show(
                    "Please fill in all required fields.",
                    "Input Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Execute the create command
            if (ViewModel.CreateBenchmarksCommand.CanExecute(null))
            {
                ViewModel.CreateBenchmarksCommand.Execute(null);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
