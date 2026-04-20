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

        private MainViewModel? VM => DataContext as MainViewModel;

        private void LibTierUnverified_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.SelectedLibraryTier = TargetLibrary.LibraryUnverified;
        }

        private void LibTierTesting_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.SelectedLibraryTier = TargetLibrary.LibraryWorkbench;
        }

        private void LibTierVerified_Checked(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.SelectedLibraryTier = TargetLibrary.Library;
        }
    }
}
