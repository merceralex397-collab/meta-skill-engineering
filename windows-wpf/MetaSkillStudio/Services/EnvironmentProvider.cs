using System;
using System.IO;
using System.Linq;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Services
{
    /// <summary>
    /// Implementation of IEnvironmentProvider for accessing environment variables and system paths.
    /// </summary>
    public class EnvironmentProvider : IEnvironmentProvider
    {
        /// <summary>
        /// Gets an environment variable by name.
        /// </summary>
        public string? GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        /// <summary>
        /// Gets a special folder path.
        /// </summary>
        public string GetFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Combines paths using Path.Combine.
        /// </summary>
        public string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Gets the directory name from a path.
        /// </summary>
        public string? GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Gets the file name from a path.
        /// </summary>
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }
    }
}
