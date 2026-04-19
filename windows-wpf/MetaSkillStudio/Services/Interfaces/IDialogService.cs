using System.Collections.Generic;
using System.Threading.Tasks;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.Services.Interfaces
{
    /// <summary>
    /// Service for displaying dialogs and UI interactions.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a message box with the specified message and title.
        /// </summary>
        void ShowMessage(string message, string title, MessageType type = MessageType.Information);

        /// <summary>
        /// Shows a dialog of type T and returns the dialog result.
        /// </summary>
        bool? ShowDialog<T>() where T : class;

        /// <summary>
        /// Shows the create skill dialog and returns the result.
        /// </summary>
        (bool? Result, string SkillBrief, TargetLibrary TargetLibrary) ShowCreateSkillDialog();

        /// <summary>
        /// Shows the skill selection dialog and returns the selected skill.
        /// </summary>
        (bool? Result, SkillInfo? SelectedSkill, bool TestAllSkills) ShowSkillSelectionDialog(List<SkillInfo> skills, string description, bool allowTestAll = false);

        /// <summary>
        /// Shows an input dialog and returns the user input.
        /// </summary>
        (bool? Result, string ResponseText) ShowInputDialog(string title, string message, string defaultResponse = "");

        /// <summary>
        /// Shows the benchmark dialog and returns the configuration.
        /// </summary>
        (bool? Result, string SkillName, string BenchmarkGoal, int CaseCount) ShowBenchmarkDialog();

        /// <summary>
        /// Shows the settings dialog.
        /// </summary>
        bool? ShowSettingsDialog();

        /// <summary>
        /// Shows the analytics dialog.
        /// </summary>
        void ShowAnalyticsDialog();

        /// <summary>
        /// Shows the run details dialog for the specified run.
        /// </summary>
        void ShowRunDetailsDialog(RunInfo runInfo);
    }

    /// <summary>
    /// Message type for dialog display.
    /// </summary>
    public enum MessageType
    {
        Information,
        Warning,
        Error,
        Question
    }
}
