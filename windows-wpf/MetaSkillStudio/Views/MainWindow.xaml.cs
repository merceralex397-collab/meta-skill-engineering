using System.Windows;
using MetaSkillStudio.Models;
using MetaSkillStudio.ViewModels;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Main application window for Meta Skill Studio.
    /// Hosts the primary user interface and view model.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private ShellViewModel? VM => DataContext as ShellViewModel;

        private void LibTierUnverified_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.LibraryPage.SelectedLibraryTier = TargetLibrary.LibraryUnverified;
        }

        private void LibTierTesting_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.LibraryPage.SelectedLibraryTier = TargetLibrary.LibraryWorkbench;
        }

        private void LibTierVerified_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.LibraryPage.SelectedLibraryTier = TargetLibrary.Library;
        }

        private void QuickStartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (VM?.OpenQuickStartCommand.CanExecute(null) == true)
            {
                VM.OpenQuickStartCommand.Execute(null);
            }
        }

        private void RefreshLibraryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (VM?.RefreshLibraryCommand.CanExecute(null) == true)
            {
                VM.RefreshLibraryCommand.Execute(null);
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Meta Skill Studio\n\nA Windows studio for importing, testing, improving, and managing AI skill packages.",
                "About Meta Skill Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
