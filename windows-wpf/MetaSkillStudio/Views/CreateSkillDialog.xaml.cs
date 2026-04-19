using System.Windows;
using MetaSkillStudio.Models;

using MessageBox = System.Windows.MessageBox;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Dialog for creating a new skill with target library selection.
    /// </summary>
    public partial class CreateSkillDialog : Window
    {
        public string SkillBrief { get; private set; } = string.Empty;
        public TargetLibrary TargetLibrary { get; private set; } = TargetLibrary.LibraryUnverified;

        public CreateSkillDialog()
        {
            InitializeComponent();
            
            // Update description when library selection changes
            UnverifiedRadio.Checked += (s, e) => UpdateDescription();
            WorkbenchRadio.Checked += (s, e) => UpdateDescription();
        }

        private void UpdateDescription()
        {
            if (WorkbenchRadio.IsChecked == true)
            {
                LibraryDescription.Text = "Skills under active evaluation and benchmark iteration live here.";
                TargetLibrary = TargetLibrary.LibraryWorkbench;
            }
            else
            {
                LibraryDescription.Text = "Raw, untested skills go here before workbench promotion.";
                TargetLibrary = TargetLibrary.LibraryUnverified;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var brief = BriefTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(brief))
            {
                MessageBox.Show("Please enter a skill brief.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SkillBrief = brief;
            TargetLibrary = WorkbenchRadio.IsChecked == true ? TargetLibrary.LibraryWorkbench : TargetLibrary.LibraryUnverified;
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
