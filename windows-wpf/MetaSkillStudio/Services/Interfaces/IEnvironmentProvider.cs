using System;
using System.IO;

namespace MetaSkillStudio.Services.Interfaces
{
    /// <summary>
    /// Provides access to environment variables and system paths.
    /// </summary>
    public interface IEnvironmentProvider
    {
        /// <summary>
        /// Gets an environment variable by name.
        /// </summary>
        string? GetEnvironmentVariable(string name);

        /// <summary>
        /// Gets a special folder path.
        /// </summary>
        string GetFolderPath(Environment.SpecialFolder folder);

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        string GetCurrentDirectory();

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        bool DirectoryExists(string path);

        /// <summary>
        /// Combines paths using Path.Combine.
        /// </summary>
        string CombinePaths(params string[] paths);

        /// <summary>
        /// Gets the directory name from a path.
        /// </summary>
        string? GetDirectoryName(string path);

        /// <summary>
        /// Gets the file name from a path.
        /// </summary>
        string GetFileName(string path);
    }
}
