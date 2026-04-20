using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MetaSkillStudio.Helpers;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Services
{
    /// <summary>
    /// Service for communicating with the Python backend and managing runtime detection.
    /// Implements IPythonRuntimeService for dependency injection.
    /// </summary>
    public class PythonRuntimeService : IPythonRuntimeService
    {
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IConfigurationStorage _configStorage;
        private readonly string _pythonPath;
        private readonly string _nodePath;
        private readonly string _repoRoot;
        private readonly string _studioScriptPath;
        private readonly string _opencodeConfigPath;
        private readonly string _opencodeSdkBridgePath;

        // PERFORMANCE FIX: Use RegexCache for all compiled regex patterns with timeouts

        private const string OpenCodeRuntimeName = "opencode";
        private static readonly string[] OpenCodeModelProbeArgs = { "--models" };

        /// <summary>
        /// Initializes a new instance of the PythonRuntimeService class.
        /// </summary>
        /// <param name="environmentProvider">The environment provider for system operations.</param>
        /// <param name="configStorage">The configuration storage for persisting settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when environmentProvider or configStorage is null.</exception>
        public PythonRuntimeService(IEnvironmentProvider environmentProvider, IConfigurationStorage configStorage)
        {
            _environmentProvider = environmentProvider ?? throw new ArgumentNullException(nameof(environmentProvider));
            _configStorage = configStorage ?? throw new ArgumentNullException(nameof(configStorage));
            
            _pythonPath = DetectPythonPath();
            _repoRoot = FindRepoRoot();
            _nodePath = DetectNodePath();
            _studioScriptPath = _environmentProvider.CombinePaths(_repoRoot, "scripts", "meta-skill-studio.py");
            _opencodeConfigPath = _environmentProvider.CombinePaths(_repoRoot, ".opencode", "opencode.json");
            _opencodeSdkBridgePath = _environmentProvider.CombinePaths(_repoRoot, "scripts", "meta_skill_studio", "opencode_sdk_bridge.mjs");
        }

        /// <summary>
        /// Detects available AI CLI runtimes on the system.
        /// </summary>
        /// <returns>A list of detected runtimes with their available models.</returns>
        public async Task<List<DetectedRuntime>> DetectRuntimesAsync()
        {
            var runtime = await DetectSingleRuntimeAsync(OpenCodeRuntimeName, OpenCodeRuntimeName, OpenCodeModelProbeArgs);
            return new List<DetectedRuntime> { runtime };
        }

        private async Task<DetectedRuntime> DetectSingleRuntimeAsync(string name, string command, string[] probeArgs)
        {
            var configuredModel = GetConfiguredOpenCodeModel();

            try
            {
                var commandPath = string.Equals(name, OpenCodeRuntimeName, StringComparison.OrdinalIgnoreCase)
                    ? FindPreferredOpenCodeCommandPath()
                    : FindCommandPath(command);
                if (string.IsNullOrEmpty(commandPath))
                {
                    return new DetectedRuntime
                    {
                        Name = name,
                        Command = string.Empty,
                        Models = MergeModels(configuredModel, new List<string>())
                    };
                }

                var models = await ProbeModelsAsync(commandPath, probeArgs);

                return new DetectedRuntime
                {
                    Name = name,
                    Command = commandPath,
                    Models = MergeModels(configuredModel, models)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PythonRuntimeService] Failed to detect runtime '{name}': {ex.Message}");
                return new DetectedRuntime
                {
                    Name = name,
                    Command = string.Empty,
                    Models = MergeModels(configuredModel, new List<string>())
                };
            }
        }

        private string? GetConfiguredOpenCodeModel()
        {
            try
            {
                if (!_environmentProvider.FileExists(_opencodeConfigPath))
                {
                    return null;
                }

                using var document = JsonDocument.Parse(File.ReadAllText(_opencodeConfigPath));
                if (document.RootElement.TryGetProperty("model", out var modelProperty) &&
                    modelProperty.ValueKind == JsonValueKind.String)
                {
                    var model = modelProperty.GetString();
                    return string.IsNullOrWhiteSpace(model) ? null : model.Trim();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PythonRuntimeService] Failed to read OpenCode config: {ex.Message}");
            }

            return null;
        }

        private static List<string> MergeModels(string? configuredModel, List<string> discoveredModels)
        {
            var merged = new List<string>();

            if (!string.IsNullOrWhiteSpace(configuredModel))
            {
                merged.Add(configuredModel);
            }

            foreach (var model in discoveredModels)
            {
                if (!string.IsNullOrWhiteSpace(model) && !merged.Contains(model))
                {
                    merged.Add(model);
                }
            }

            return merged.Any() ? merged : new List<string> { "auto" };
        }

        private async Task<List<string>> ProbeModelsAsync(string commandPath, string[] probeArgs)
        {
            var models = new List<string>();

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = commandPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _repoRoot
                };

                foreach (var arg in probeArgs)
                {
                    psi.ArgumentList.Add(arg);
                }

                using var process = Process.Start(psi);
                if (process == null) return models;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                try
                {
                    var output = await process.StandardOutput.ReadToEndAsync(cts.Token);
                    await process.WaitForExitAsync(cts.Token);

                    if (process.ExitCode == 0)
                    {
                        models = ExtractModelsFromOutput(output);
                    }
                }
                catch (OperationCanceledException)
                {
                    try { process.Kill(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PythonRuntimeService] Failed to probe models: {ex.Message}");
                // Probe failed, return empty list
            }
            return models;
        }

        private List<string> ExtractModelsFromOutput(string output)
        {
            // PERFORMANCE FIX: Pre-size the list - we know the max is 30 models
            var models = new List<string>(30);
            var stopTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "usage", "options", "help", "model", "models", "default",
                "version", "reasoning", "output", "input", "json", "yaml"
            };

            // PERFORMANCE FIX: Use Span for efficient line parsing without allocating substrings
            ReadOnlySpan<char> outputSpan = output.AsSpan();
            var lineStart = 0;

            for (var i = 0; i <= outputSpan.Length; i++)
            {
                if (i == outputSpan.Length || outputSpan[i] == '\n')
                {
                    var lineLength = i - lineStart;
                    if (lineLength > 0 && lineLength <= 120)
                    {
                        var trimmed = outputSpan.Slice(lineStart, lineLength).Trim();
                        if (trimmed.Length > 0)
                        {
                            ProcessLineForModels(trimmed, stopTokens, models);
                        }
                    }
                    lineStart = i + 1;
                }

                if (models.Count >= 30)
                {
                    break;
                }
            }

            return models;
        }

        private static void ProcessLineForModels(ReadOnlySpan<char> line, HashSet<string> stopTokens, List<string> models)
        {
            // PERFORMANCE FIX: Use compiled regex from RegexCache
            var lineString = line.ToString();
            var matches = RegexCache.ModelIdentifier.Matches(lineString);

            foreach (Match match in matches)
            {
                var matchValue = match.Value;
                var token = matchValue.ToLowerInvariant();
                if (stopTokens.Contains(token)) continue;
                if (RegexCache.NumericOnlyPattern.IsMatch(matchValue)) continue;

                // PERFORMANCE FIX: Use static char array to avoid allocation on every iteration
                var cleanToken = matchValue.Trim(RegexCache.TrimChars);
                if (!string.IsNullOrEmpty(cleanToken) && !models.Contains(cleanToken))
                {
                    models.Add(cleanToken);
                    if (models.Count >= 30) return; // Early exit when we have enough
                }
            }
        }

        /// <summary>
        /// Loads the application configuration from storage.
        /// </summary>
        /// <returns>The loaded configuration, or null if no configuration exists.</returns>
        public AppConfiguration? LoadConfiguration()
        {
            var config = _configStorage.Load();
            if (config == null)
            {
                return null;
            }

            var configuredModel = GetConfiguredOpenCodeModel() ?? "auto";
            config.DetectedRuntimes = config.DetectedRuntimes
                .Where(runtime => string.Equals(runtime.Name, OpenCodeRuntimeName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var role in AppConfiguration.GetDefaultRoles().Keys)
            {
                if (!config.Roles.TryGetValue(role, out var roleConfig))
                {
                    config.Roles[role] = new RoleConfiguration
                    {
                        Runtime = OpenCodeRuntimeName,
                        Model = configuredModel
                    };
                    continue;
                }

                roleConfig.Runtime = OpenCodeRuntimeName;
                if (string.IsNullOrWhiteSpace(roleConfig.Model))
                {
                    roleConfig.Model = configuredModel;
                }
            }

            return config;
        }

        /// <summary>
        /// Saves the application configuration to storage.
        /// </summary>
        /// <param name="config">The configuration to save.</param>
        /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
        public void SaveConfiguration(AppConfiguration config)
        {
            _configStorage.Save(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Creates a default application configuration with detected runtimes.
        /// </summary>
        /// <returns>The newly created default configuration.</returns>
        public async Task<AppConfiguration> CreateDefaultConfigurationAsync()
        {
            var runtimes = await DetectRuntimesAsync();
            var firstRuntime = runtimes.FirstOrDefault(r => r.IsAvailable);
            var defaultModel = firstRuntime?.DefaultModel ?? "auto";

            var config = new AppConfiguration
            {
                DetectedRuntimes = runtimes,
                Roles = new Dictionary<string, RoleConfiguration>()
            };

            foreach (var role in new[] { "create", "improve", "test", "orchestrate", "judge" })
            {
                config.Roles[role] = new RoleConfiguration
                {
                    Runtime = OpenCodeRuntimeName,
                    Model = defaultModel
                };
            }

            SaveConfiguration(config);
            return config;
        }

        /// <summary>
        /// Lists all skills found in the repository.
        /// </summary>
        /// <returns>A list of skill information objects for discovered skills.</returns>
        public List<SkillInfo> ListSkills()
        {
            try
            {
                var allDirectories = Directory.GetDirectories(_repoRoot);
                // PERFORMANCE FIX: Pre-filter and sort - use StringComparison.Ordinal for efficiency
                var matchingDirs = allDirectories
                    .Where(d =>
                    {
                        var name = Path.GetFileName(d);
                        return name.StartsWith("skill-", StringComparison.Ordinal) ||
                               name.StartsWith("community-", StringComparison.Ordinal);
                    })
                    .OrderBy(d => Path.GetFileName(d))
                    .ToList();

                // PERFORMANCE FIX: Pre-size the list based on estimated count
                var skills = new List<SkillInfo>(matchingDirs.Count);

                foreach (var dir in matchingDirs)
                {
                    var name = Path.GetFileName(dir);
                    var skillMdPath = Path.Combine(dir, "SKILL.md");
                    var evalsDir = Path.Combine(dir, "evals");

                    if (File.Exists(skillMdPath))
                    {
                        var description = ExtractSkillDescription(skillMdPath);
                        skills.Add(new SkillInfo
                        {
                            Name = name,
                            DirectoryPath = dir,
                            Description = description,
                            HasSkillMd = true,
                            HasEvals = Directory.Exists(evalsDir)
                        });
                    }
                }

                return skills;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error listing skills: {ex.Message}");
                return new List<SkillInfo>();
            }
        }

        private string? ExtractSkillDescription(string skillMdPath)
        {
            try
            {
                var content = File.ReadAllText(skillMdPath);

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var frontmatterMatch = RegexCache.FrontmatterDescription.Match(content);
                if (frontmatterMatch.Success)
                {
                    return frontmatterMatch.Groups[1].Value.Trim();
                }

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var purposeMatch = RegexCache.PurposeSection.Match(content);
                if (purposeMatch.Success)
                {
                    var purpose = purposeMatch.Groups[1].Value.Trim();
                    // PERFORMANCE FIX: Use Span for zero-allocation first sentence extraction
                    var purposeSpan = purpose.AsSpan();
                    var endPos = 0;
                    while (endPos < purposeSpan.Length && purposeSpan[endPos] != '.' && purposeSpan[endPos] != '\n')
                    {
                        endPos++;
                    }
                    return purposeSpan.Slice(0, endPos).Trim().ToString();
                }
            }
            catch
            {
                // Ignore extraction errors
            }

            return null;
        }

        /// <summary>
        /// Executes a Python command with the specified action and parameters.
        /// SECURITY FIX: Uses ArgumentList to prevent command injection.
        /// </summary>
        /// <param name="action">The action to perform (e.g., "create", "improve", "test").</param>
        /// <param name="parameter">The parameter for the action (varies by action type).</param>
        /// <param name="library">The target library for the operation. Defaults to LibraryUnverified.</param>
        /// <param name="benchmarkCases">Optional number of benchmark cases for benchmarks action.</param>
        /// <returns>The result of the command execution.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the studio script is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the Python process fails to start.</exception>
        /// <exception cref="TimeoutException">Thrown when execution exceeds the 30-minute timeout.</exception>
        public async Task<RunResult> ExecuteCommandAsync(string action, string parameter, TargetLibrary library = TargetLibrary.LibraryUnverified, int? benchmarkCases = null)
        {
            if (string.Equals(action, "assistant", StringComparison.OrdinalIgnoreCase))
            {
                return await ExecuteAssistantCommandAsync(parameter);
            }

            if (!_environmentProvider.FileExists(_studioScriptPath))
            {
                throw new FileNotFoundException("Studio script not found", _studioScriptPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = _pythonPath,
                WorkingDirectory = _repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // SECURITY FIX: Use ArgumentList instead of string-based Arguments
            // This prevents command injection by bypassing shell interpretation entirely
            psi.ArgumentList.Add(_studioScriptPath);

            // Add action-specific arguments using ArgumentList
            AddArgumentsToList(psi.ArgumentList, action, parameter, library, benchmarkCases);

            var startTime = DateTime.UtcNow;
            using var process = Process.Start(psi);

            if (process == null)
            {
                throw new InvalidOperationException("Failed to start Python process");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            // SECURITY FIX: Add timeout with CancellationTokenSource and process.Kill()
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // SECURITY FIX: Kill process when timeout expires
                try { process.Kill(); } catch { /* Best effort */ }
                throw new TimeoutException("Command execution exceeded 30-minute timeout");
            }

            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalSeconds;

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            return new RunResult
            {
                Action = action,
                ExitCode = process.ExitCode,
                Stdout = stdout,
                Stderr = stderr,
                DurationSeconds = Math.Round(duration, 3),
                StartedAtUtc = startTime,
                EndedAtUtc = endTime,
                Input = new Dictionary<string, object> { ["action"] = action, ["parameter"] = parameter },
                Artifacts = new Dictionary<string, object>()
            };
        }

        private async Task<RunResult> ExecuteAssistantCommandAsync(string prompt)
        {
            if (!_environmentProvider.FileExists(_opencodeSdkBridgePath))
            {
                throw new FileNotFoundException("OpenCode SDK bridge script not found", _opencodeSdkBridgePath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = _nodePath,
                WorkingDirectory = _repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add(_opencodeSdkBridgePath);
            psi.ArgumentList.Add("assistant");
            psi.ArgumentList.Add("--prompt");
            psi.ArgumentList.Add(prompt);

            var startTime = DateTime.UtcNow;
            using var process = Process.Start(psi);

            if (process == null)
            {
                throw new InvalidOperationException("Failed to start OpenCode SDK bridge");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(); } catch { }
                throw new TimeoutException("OpenCode assistant request exceeded the 10-minute timeout.");
            }

            var endTime = DateTime.UtcNow;
            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            var outputText = stdout.Trim();
            var artifacts = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                try
                {
                    using var document = JsonDocument.Parse(stdout);

                    if (document.RootElement.TryGetProperty("response", out var responseProperty) &&
                        responseProperty.ValueKind == JsonValueKind.String)
                    {
                        outputText = responseProperty.GetString() ?? string.Empty;
                    }

                    if (document.RootElement.TryGetProperty("sessionId", out var sessionProperty) &&
                        sessionProperty.ValueKind == JsonValueKind.String)
                    {
                        artifacts["sessionId"] = sessionProperty.GetString() ?? string.Empty;
                    }

                    if (document.RootElement.TryGetProperty("model", out var modelProperty) &&
                        modelProperty.ValueKind == JsonValueKind.String)
                    {
                        artifacts["model"] = modelProperty.GetString() ?? string.Empty;
                    }
                }
                catch (JsonException)
                {
                    // Preserve the raw bridge output if JSON parsing fails.
                }
            }

            return new RunResult
            {
                Action = "assistant",
                ExitCode = process.ExitCode,
                Stdout = outputText,
                Stderr = stderr,
                DurationSeconds = Math.Round((endTime - startTime).TotalSeconds, 3),
                StartedAtUtc = startTime,
                EndedAtUtc = endTime,
                Input = new Dictionary<string, object>
                {
                    ["action"] = "assistant",
                    ["parameter"] = prompt,
                },
                Artifacts = artifacts,
            };
        }

        /// <summary>
        /// Adds arguments to ArgumentList based on action type.
        /// SECURITY FIX: Uses ArgumentList.Add() instead of string concatenation
        /// This prevents command injection vulnerabilities.
        /// </summary>
        private void AddArgumentsToList(IList<string> argumentList, string action, string parameter, TargetLibrary library, int? benchmarkCases)
        {
            var libraryName = library == TargetLibrary.LibraryWorkbench ? "LibraryWorkbench" : "LibraryUnverified";

            switch (action.ToLower())
            {
                case "create":
                    argumentList.Add("--mode");
                    argumentList.Add("cli");
                    argumentList.Add("--action");
                    argumentList.Add("create");
                    argumentList.Add("--brief");
                    argumentList.Add(parameter); // No escaping needed with ArgumentList
                    argumentList.Add("--library");
                    argumentList.Add(libraryName);
                    break;

                case "improve":
                    var improveParts = parameter.Split('|', 2);
                    if (improveParts.Length != 2)
                        throw new ArgumentException("Improve parameter must be 'skill|goal' format");
                    argumentList.Add("--mode");
                    argumentList.Add("cli");
                    argumentList.Add("--action");
                    argumentList.Add("improve");
                    argumentList.Add("--skill");
                    argumentList.Add(improveParts[0]);
                    argumentList.Add("--goal");
                    argumentList.Add(improveParts[1]);
                    break;

                case "test":
                    argumentList.Add("--mode");
                    argumentList.Add("cli");
                    argumentList.Add("--action");
                    argumentList.Add("test");
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        argumentList.Add("--skill");
                        argumentList.Add(parameter);
                    }
                    break;

                case "meta-manage":
                    argumentList.Add("--mode");
                    argumentList.Add("cli");
                    argumentList.Add("--action");
                    argumentList.Add("meta-manage");
                    argumentList.Add("--goal");
                    argumentList.Add(parameter);
                    break;

                case "benchmarks":
                    var benchParts = parameter.Split('|', 2);
                    if (benchParts.Length != 2)
                        throw new ArgumentException("Benchmarks parameter must be 'skill|goal' format");
                    argumentList.Add("--mode");
                    argumentList.Add("cli");
                    argumentList.Add("--action");
                    argumentList.Add("benchmarks");
                    argumentList.Add("--skill");
                    argumentList.Add(benchParts[0]);
                    argumentList.Add("--goal");
                    argumentList.Add(benchParts[1]);
                    argumentList.Add("--cases");
                    argumentList.Add((benchmarkCases ?? 8).ToString());
                    break;

                case "reconfigure":
                    argumentList.Add("--setup");
                    break;

                default:
                    throw new ArgumentException($"Unknown action: {action}");
            }
        }

        // SECURITY FIX: Removed unsafe EscapeArgument method entirely
        // Command injection prevention is now handled by using ProcessStartInfo.ArgumentList
        // which bypasses shell interpretation entirely.

        /// <summary>
        /// Parses the judge output from a test execution to extract quality metrics.
        /// </summary>
        /// <param name="output">The stdout text from the test execution.</param>
        /// <returns>A JudgeResult containing parsed metrics, or null if parsing fails.</returns>
        public JudgeResult? ParseJudgeOutput(string output)
        {
            try
            {
                var result = new JudgeResult();

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var scoreMatch = RegexCache.QualityScore.Match(output);
                if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var score))
                {
                    result.QualityScore = score;
                }

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var routingMatch = RegexCache.RoutingQualityNotes.Match(output);
                if (routingMatch.Success)
                {
                    result.RoutingNotes = routingMatch.Groups[1].Value.Trim();
                }

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var behaviorMatch = RegexCache.BehaviorQualityNotes.Match(output);
                if (behaviorMatch.Success)
                {
                    result.BehaviorNotes = behaviorMatch.Groups[1].Value.Trim();
                }

                // PERFORMANCE FIX: Use compiled regex from RegexCache with timeout protection
                var fixesMatch = RegexCache.HighestPriorityFixes.Match(output);
                if (fixesMatch.Success)
                {
                    result.PriorityFixes = ParsePriorityFixes(fixesMatch.Groups[1].Value.Trim());
                }

                if (!result.QualityScore.HasValue
                    && string.IsNullOrWhiteSpace(result.RoutingNotes)
                    && string.IsNullOrWhiteSpace(result.BehaviorNotes)
                    && (result.PriorityFixes == null || result.PriorityFixes.Count == 0))
                {
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PythonRuntimeService] Failed to parse judge output: {ex.Message}");
                return null;
            }
        }

        private static List<string> ParsePriorityFixes(string fixesText)
        {
            // PERFORMANCE FIX: Pre-size with reasonable estimate
            var fixes = new List<string>(10);

            // PERFORMANCE FIX: Use Span for zero-allocation line parsing
            var span = fixesText.AsSpan();
            var start = 0;

            for (var i = 0; i <= span.Length; i++)
            {
                if (i == span.Length || span[i] == '\n')
                {
                    var lineLength = i - start;
                    if (lineLength > 0)
                    {
                        var line = span.Slice(start, lineLength).Trim();
                        if (line.Length > 0)
                        {
                            // Trim leading '-', '*', ' ', '\t' without allocation
                            var j = 0;
                            while (j < line.Length && (line[j] == '-' || line[j] == '*' || line[j] == ' ' || line[j] == '\t'))
                            {
                                j++;
                            }
                            if (j < line.Length)
                            {
                                var clean = line.Slice(j).ToString();
                                if (!string.IsNullOrWhiteSpace(clean))
                                {
                                    fixes.Add(clean);
                                }
                            }
                        }
                    }
                    start = i + 1;
                }
            }

            return fixes;
        }

        private string DetectPythonPath()
        {
            var candidates = new[]
            {
                "python",
                "python3",
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                _environmentProvider.CombinePaths(
                    _environmentProvider.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Microsoft", "WindowsApps", "python.exe"),
                _environmentProvider.CombinePaths(
                    _environmentProvider.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                    "Python311", "python.exe"),
                _environmentProvider.CombinePaths(
                    _environmentProvider.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                    "Python312", "python.exe"),
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var path = FindCommandPath(candidate);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = path,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        // SECURITY FIX: Use ArgumentList instead of Arguments string
                        psi.ArgumentList.Add("--version");

                        using var process = Process.Start(psi);
                        if (process?.WaitForExit(5000) == true && process.ExitCode == 0)
                        {
                            return path;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PythonRuntimeService] Python detection candidate failed: {ex.Message}");
                    // Try next candidate
                }
            }

            return "python";
        }

        private string DetectNodePath()
        {
            var candidates = new[]
            {
                "node",
                @"C:\Program Files\nodejs\node.exe",
                @"C:\Program Files (x86)\nodejs\node.exe",
                _environmentProvider.CombinePaths(
                    _environmentProvider.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "nodejs",
                    "node.exe"),
            };

            foreach (var candidate in candidates)
            {
                var path = FindCommandPath(candidate);
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }

            return "node";
        }

        private string? FindPreferredOpenCodeCommandPath()
        {
            foreach (var candidate in GetLocalOpenCodeCandidates())
            {
                if (_environmentProvider.FileExists(candidate))
                {
                    return candidate;
                }
            }

            return FindCommandPath(OpenCodeRuntimeName);
        }

        private IEnumerable<string> GetLocalOpenCodeCandidates()
        {
            if (OperatingSystem.IsWindows())
            {
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", "opencode-windows-x64", "bin", "opencode.exe");
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", "opencode-windows-x64-baseline", "bin", "opencode.exe");
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", ".bin", "opencode.cmd");
            }
            else
            {
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", "opencode-linux-x64", "bin", "opencode");
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", "opencode-linux-arm64", "bin", "opencode");
                yield return _environmentProvider.CombinePaths(_repoRoot, ".opencode", "node_modules", ".bin", "opencode");
            }
        }

        private string? FindCommandPath(string command)
        {
            if (Path.IsPathRooted(command) && _environmentProvider.FileExists(command))
                return command;

            var pathEnv = _environmentProvider.GetEnvironmentVariable("PATH") ?? "";
            var paths = pathEnv.Split(Path.PathSeparator);

            foreach (var dir in paths)
            {
                var fullPath = Path.Combine(dir, command);
                if (_environmentProvider.FileExists(fullPath))
                    return fullPath;

                if (OperatingSystem.IsWindows())
                {
                    var withExe = fullPath + ".exe";
                    if (_environmentProvider.FileExists(withExe))
                        return withExe;

                    var withCmd = fullPath + ".cmd";
                    if (_environmentProvider.FileExists(withCmd))
                        return withCmd;

                    var withBat = fullPath + ".bat";
                    if (_environmentProvider.FileExists(withBat))
                        return withBat;
                }
            }

            return null;
        }

        private string FindRepoRoot()
        {
            foreach (var variableName in new[] { "META_SKILL_STUDIO_REPO_ROOT", "META_SKILL_REPO_ROOT" })
            {
                var configuredRoot = _environmentProvider.GetEnvironmentVariable(variableName);
                if (IsRepoRoot(configuredRoot))
                {
                    return configuredRoot!;
                }
            }

            var configDirectory = _environmentProvider.GetDirectoryName(_configStorage.GetConfigPath());

            foreach (var candidate in new[]
            {
                _environmentProvider.GetCurrentDirectory(),
                configDirectory,
                AppContext.BaseDirectory,
            })
            {
                var repoRoot = TryFindRepoRoot(candidate);
                if (!string.IsNullOrEmpty(repoRoot))
                {
                    return repoRoot;
                }
            }

            throw new DirectoryNotFoundException("Could not find Meta-Skill-Engineering repository root (AGENTS.md not found)");
        }

        private string? TryFindRepoRoot(string? startPath)
        {
            var current = NormalizeDirectoryCandidate(startPath);

            while (!string.IsNullOrEmpty(current))
            {
                if (IsRepoRoot(current))
                {
                    return current;
                }

                current = _environmentProvider.GetDirectoryName(current);
            }

            return null;
        }

        private string? NormalizeDirectoryCandidate(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (_environmentProvider.DirectoryExists(path))
            {
                return path;
            }

            return _environmentProvider.GetDirectoryName(path);
        }

        private bool IsRepoRoot(string? path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && _environmentProvider.FileExists(_environmentProvider.CombinePaths(path!, "AGENTS.md"));
        }
    }
}
