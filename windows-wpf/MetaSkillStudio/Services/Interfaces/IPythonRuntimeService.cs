using System.Collections.Generic;
using System.Threading.Tasks;
using MetaSkillStudio.Models;

namespace MetaSkillStudio.Services.Interfaces
{
    /// <summary>
    /// Service for communicating with the Python backend and managing runtime detection.
    /// </summary>
    public interface IPythonRuntimeService
    {
        /// <summary>
        /// Detects all available AI CLI runtimes on the system.
        /// </summary>
        Task<List<DetectedRuntime>> DetectRuntimesAsync();

        /// <summary>
        /// Loads the application configuration from disk.
        /// </summary>
        AppConfiguration? LoadConfiguration();

        /// <summary>
        /// Saves the application configuration to disk.
        /// </summary>
        void SaveConfiguration(AppConfiguration config);

        /// <summary>
        /// Creates a default configuration using detected runtimes.
        /// </summary>
        Task<AppConfiguration> CreateDefaultConfigurationAsync();

        /// <summary>
        /// Lists all available skills in the repository.
        /// </summary>
        List<SkillInfo> ListSkills();

        /// <summary>
        /// Executes a Python command with the specified action and parameters.
        /// </summary>
        Task<RunResult> ExecuteCommandAsync(string action, string parameter, TargetLibrary library = TargetLibrary.LibraryUnverified, int? benchmarkCases = null);

        /// <summary>
        /// Parses judge output from a run result to extract quality metrics.
        /// </summary>
        JudgeResult? ParseJudgeOutput(string output);
    }
}
