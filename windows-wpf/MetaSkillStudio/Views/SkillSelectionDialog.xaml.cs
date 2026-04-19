using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MetaSkillStudio.Models;

using MessageBox = System.Windows.MessageBox;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Dialog for selecting a skill from the available skills list.
    /// </summary>
    public partial class SkillSelectionDialog : Window
    {
        private List<SkillInfo> _allSkills = new();
        private List<SkillInfo> _filteredSkills = new();

        public SkillInfo? SelectedSkill { get; private set; }
        public bool TestAllSkills { get; private set; }

        public SkillSelectionDialog(List<SkillInfo> skills, string description = "Choose a skill from the list below.", bool allowTestAll = false)
        {
            InitializeComponent();
            _allSkills = skills;
            _filteredSkills = skills;
            DescriptionText.Text = description;
            TestAllCheckBox.Visibility = allowTestAll ? Visibility.Visible : Visibility.Collapsed;
            RefreshSkillList();
        }

        private void RefreshSkillList()
        {
            SkillListBox.ItemsSource = null;
            SkillListBox.ItemsSource = _filteredSkills;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredSkills = _allSkills;
            }
            else
            {
                _filteredSkills = _allSkills
                    .Where(s => s.Name.ToLowerInvariant().Contains(searchText) ||
                               (s.Description?.ToLowerInvariant().Contains(searchText) ?? false))
                    .ToList();
            }
            RefreshSkillList();
        }

        private void SkillListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSkill = SkillListBox.SelectedItem as SkillInfo;
            if (SelectedSkill != null)
            {
                TestAllCheckBox.IsChecked = false;
                TestAllSkills = false;
            }
        }

        private void TestAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TestAllSkills = true;
            SkillListBox.SelectedItem = null;
            SelectedSkill = null;
        }

        private void TestAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TestAllSkills = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (TestAllCheckBox.IsVisible && TestAllCheckBox.IsChecked == true)
            {
                TestAllSkills = true;
                DialogResult = true;
                Close();
                return;
            }

            if (SelectedSkill == null)
            {
                MessageBox.Show("Please select a skill or check 'Test all skills'.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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
