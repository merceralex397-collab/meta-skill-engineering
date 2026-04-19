using System;
using System.Collections.Generic;
using System.IO;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IEnvironmentProvider for testing.
    /// Provides controlled environment variables and paths.
    /// </summary>
    public class MockEnvironmentProvider : IEnvironmentProvider
    {
        private readonly Dictionary<string, string> _environmentVariables = new();
        private readonly Dictionary<Environment.SpecialFolder, string> _specialFolders = new();
        private readonly Dictionary<string, bool> _fileExists = new();
        private readonly Dictionary<string, bool> _directoryExists = new();

        private string _currentDirectory = Directory.GetCurrentDirectory();

        // Tracking properties
        public int GetEnvironmentVariableCallCount { get; private set; }
        public int GetFolderPathCallCount { get; private set; }
        public int GetCurrentDirectoryCallCount { get; private set; }
        public int FileExistsCallCount { get; private set; }
        public int DirectoryExistsCallCount { get; private set; }
        public int CombinePathsCallCount { get; private set; }
        public int GetDirectoryNameCallCount { get; private set; }
        public int GetFileNameCallCount { get; private set; }

        public void SetEnvironmentVariable(string name, string value) => _environmentVariables[name] = value;
        public void SetSpecialFolder(Environment.SpecialFolder folder, string path) => _specialFolders[folder] = path;
        public void SetFileExists(string path, bool exists) => _fileExists[path] = exists;
        public void SetDirectoryExists(string path, bool exists) => _directoryExists[path] = exists;
        public void SetCurrentDirectory(string path) => _currentDirectory = path;

        public string? GetEnvironmentVariable(string name)
        {
            GetEnvironmentVariableCallCount++;
            return _environmentVariables.TryGetValue(name, out var value) ? value : null;
        }

        public string GetFolderPath(Environment.SpecialFolder folder)
        {
            GetFolderPathCallCount++;
            return _specialFolders.TryGetValue(folder, out var path) ? path : $"C:\\Mock\\{folder}";
        }

        public string GetCurrentDirectory()
        {
            GetCurrentDirectoryCallCount++;
            return _currentDirectory;
        }

        public bool FileExists(string path)
        {
            FileExistsCallCount++;
            return _fileExists.TryGetValue(path, out var exists) ? exists : File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            DirectoryExistsCallCount++;
            return _directoryExists.TryGetValue(path, out var exists) ? exists : Directory.Exists(path);
        }

        public string CombinePaths(params string[] paths)
        {
            CombinePathsCallCount++;
            return Path.Combine(paths);
        }

        public string? GetDirectoryName(string path)
        {
            GetDirectoryNameCallCount++;
            return Path.GetDirectoryName(path);
        }

        public string GetFileName(string path)
        {
            GetFileNameCallCount++;
            return Path.GetFileName(path);
        }

        public void Reset()
        {
            _environmentVariables.Clear();
            _specialFolders.Clear();
            _fileExists.Clear();
            _directoryExists.Clear();
            _currentDirectory = Directory.GetCurrentDirectory();

            GetEnvironmentVariableCallCount = 0;
            GetFolderPathCallCount = 0;
            GetCurrentDirectoryCallCount = 0;
            FileExistsCallCount = 0;
            DirectoryExistsCallCount = 0;
            CombinePathsCallCount = 0;
            GetDirectoryNameCallCount = 0;
            GetFileNameCallCount = 0;
        }
    }
}
