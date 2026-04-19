using MetaSkillStudio.Models;

namespace MetaSkillStudio.Services.Interfaces
{
    /// <summary>
    /// Service for loading and saving application configuration.
    /// </summary>
    public interface IConfigurationStorage
    {
        /// <summary>
        /// Loads the application configuration from storage.
        /// </summary>
        AppConfiguration? Load();

        /// <summary>
        /// Saves the application configuration to storage.
        /// </summary>
        void Save(AppConfiguration config);

        /// <summary>
        /// Gets the path to the configuration file.
        /// </summary>
        string GetConfigPath();
    }
}
