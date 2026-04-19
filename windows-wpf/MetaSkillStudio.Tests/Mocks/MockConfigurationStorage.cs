using System.Collections.Generic;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IConfigurationStorage for testing.
    /// Uses in-memory storage instead of file system.
    /// </summary>
    public class MockConfigurationStorage : IConfigurationStorage
    {
        private AppConfiguration? _storedConfig;
        private readonly string _configPath;

        // Tracking properties
        public int LoadCallCount { get; private set; }
        public int SaveCallCount { get; private set; }
        public int GetConfigPathCallCount { get; private set; }

        // Configuration
        public bool ShouldThrowOnLoad { get; set; }
        public bool ShouldThrowOnSave { get; set; }

        // Access to stored configuration for verification
        public AppConfiguration? StoredConfiguration => _storedConfig;
        public List<AppConfiguration> SaveHistory { get; } = new();

        public MockConfigurationStorage(string configPath = "C:\\Mock\\config.json")
        {
            _configPath = configPath;
        }

        public AppConfiguration? Load()
        {
            LoadCallCount++;

            if (ShouldThrowOnLoad)
                throw new System.IO.IOException("Mock load failure");

            return _storedConfig;
        }

        public void Save(AppConfiguration config)
        {
            SaveCallCount++;
            SaveHistory.Add(config);

            if (ShouldThrowOnSave)
                throw new System.IO.IOException("Mock save failure");

            _storedConfig = config;
        }

        public string GetConfigPath()
        {
            GetConfigPathCallCount++;
            return _configPath;
        }

        public void SetStoredConfiguration(AppConfiguration config)
        {
            _storedConfig = config;
        }

        public void Reset()
        {
            _storedConfig = null;
            LoadCallCount = 0;
            SaveCallCount = 0;
            GetConfigPathCallCount = 0;
            SaveHistory.Clear();
        }
    }
}
