using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Services
{
    /// <summary>
    /// Implementation of IConfigurationStorage for loading and saving application configuration.
    /// </summary>
    public class ConfigurationStorage : IConfigurationStorage
    {
        private readonly IEnvironmentProvider _environmentProvider;
        private string? _cachedConfigPath;

        public ConfigurationStorage(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider ?? throw new ArgumentNullException(nameof(environmentProvider));
        }

        /// <summary>
        /// Gets the path to the configuration file.
        /// </summary>
        public string GetConfigPath()
        {
            if (_cachedConfigPath != null)
                return _cachedConfigPath;

            var userProfile = _environmentProvider.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _cachedConfigPath = _environmentProvider.CombinePaths(userProfile, ".meta-skill-studio", "config.json");
            return _cachedConfigPath;
        }

        /// <summary>
        /// Loads the application configuration from storage.
        /// </summary>
        public AppConfiguration? Load()
        {
            try
            {
                var configPath = GetConfigPath();
                if (!_environmentProvider.FileExists(configPath))
                    return null;

                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigurationStorage] Failed to load configuration: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves the application configuration to storage.
        /// </summary>
        public void Save(AppConfiguration config)
        {
            try
            {
                var configPath = GetConfigPath();
                var configDir = _environmentProvider.GetDirectoryName(configPath);
                
                if (!string.IsNullOrEmpty(configDir) && !_environmentProvider.DirectoryExists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                config.LastUpdatedUtc = DateTime.UtcNow;
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }
    }
}
