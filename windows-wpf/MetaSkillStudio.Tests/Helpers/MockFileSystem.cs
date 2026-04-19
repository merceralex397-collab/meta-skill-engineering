using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MetaSkillStudio.Tests.Helpers
{
    /// <summary>
    /// Mock file system for testing file operations without touching the actual file system.
    /// </summary>
    public class MockFileSystem
    {
        private readonly Dictionary<string, string> _files = new();
        private readonly Dictionary<string, HashSet<string>> _directories = new();
        private readonly Dictionary<string, byte[]> _binaryFiles = new();

        // Track operations
        public List<string> CreatedFiles { get; } = new();
        public List<string> DeletedFiles { get; } = new();
        public List<string> CreatedDirectories { get; } = new();
        public int ReadFileCount { get; private set; }
        public int WriteFileCount { get; private set; }

        public void AddFile(string path, string content)
        {
            var normalizedPath = NormalizePath(path);
            _files[normalizedPath] = content;
            CreatedFiles.Add(normalizedPath);
            
            // Ensure directory exists
            var dir = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(dir))
            {
                EnsureDirectoryExists(dir);
            }
        }

        public void AddFile(string path, byte[] content)
        {
            var normalizedPath = NormalizePath(path);
            _binaryFiles[normalizedPath] = content;
            CreatedFiles.Add(normalizedPath);
        }

        public void AddDirectory(string path)
        {
            var normalizedPath = NormalizePath(path);
            if (!_directories.ContainsKey(normalizedPath))
            {
                _directories[normalizedPath] = new HashSet<string>();
                CreatedDirectories.Add(normalizedPath);
            }
        }

        public bool FileExists(string path)
        {
            var normalizedPath = NormalizePath(path);
            return _files.ContainsKey(normalizedPath) || _binaryFiles.ContainsKey(normalizedPath);
        }

        public bool DirectoryExists(string path)
        {
            var normalizedPath = NormalizePath(path);
            return _directories.ContainsKey(normalizedPath);
        }

        public string ReadAllText(string path)
        {
            ReadFileCount++;
            var normalizedPath = NormalizePath(path);
            
            if (_files.TryGetValue(normalizedPath, out var content))
            {
                return content;
            }
            
            throw new FileNotFoundException($"File not found: {path}");
        }

        public byte[] ReadAllBytes(string path)
        {
            ReadFileCount++;
            var normalizedPath = NormalizePath(path);
            
            if (_binaryFiles.TryGetValue(normalizedPath, out var content))
            {
                return content;
            }
            
            throw new FileNotFoundException($"File not found: {path}");
        }

        public void WriteAllText(string path, string content)
        {
            WriteFileCount++;
            var normalizedPath = NormalizePath(path);
            _files[normalizedPath] = content;
            CreatedFiles.Add(normalizedPath);

            var dir = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(dir))
            {
                EnsureDirectoryExists(dir);
                var parentDir = NormalizePath(dir);
                if (_directories.ContainsKey(parentDir))
                {
                    _directories[parentDir].Add(Path.GetFileName(normalizedPath));
                }
            }
        }

        public void DeleteFile(string path)
        {
            var normalizedPath = NormalizePath(path);
            _files.Remove(normalizedPath);
            _binaryFiles.Remove(normalizedPath);
            DeletedFiles.Add(normalizedPath);
        }

        public string[] GetFiles(string directoryPath, string searchPattern = "*")
        {
            var normalizedDir = NormalizePath(directoryPath);
            var pattern = searchPattern.Replace("*", "");
            
            var files = _files.Keys
                .Concat(_binaryFiles.Keys)
                .Where(f => f.StartsWith(normalizedDir))
                .Where(f => string.IsNullOrEmpty(pattern) || Path.GetFileName(f).Contains(pattern))
                .ToArray();

            return files;
        }

        public string[] GetDirectories(string path)
        {
            var normalizedPath = NormalizePath(path);
            return _directories.Keys.Where(d => d.StartsWith(normalizedPath)).ToArray();
        }

        private void EnsureDirectoryExists(string path)
        {
            var normalizedPath = NormalizePath(path);
            if (!_directories.ContainsKey(normalizedPath))
            {
                _directories[normalizedPath] = new HashSet<string>();
                CreatedDirectories.Add(normalizedPath);

                // Ensure parent directories
                var parent = Path.GetDirectoryName(normalizedPath);
                if (!string.IsNullOrEmpty(parent))
                {
                    EnsureDirectoryExists(parent);
                    var parentNormalized = NormalizePath(parent);
                    if (_directories.ContainsKey(parentNormalized))
                    {
                        _directories[parentNormalized].Add(Path.GetFileName(normalizedPath));
                    }
                }
            }
        }

        private static string NormalizePath(string path)
        {
            return path.Replace("\\", "/").TrimEnd('/');
        }

        public void Reset()
        {
            _files.Clear();
            _directories.Clear();
            _binaryFiles.Clear();
            CreatedFiles.Clear();
            DeletedFiles.Clear();
            CreatedDirectories.Clear();
            ReadFileCount = 0;
            WriteFileCount = 0;
        }

        public IReadOnlyDictionary<string, string> Files => _files;
        public IReadOnlyDictionary<string, HashSet<string>> Directories => _directories;
    }
}
