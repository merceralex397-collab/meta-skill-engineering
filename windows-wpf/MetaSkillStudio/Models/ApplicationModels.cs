using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MetaSkillStudio.Models
{
    /// <summary>
    /// JSON converter for DateTime UTC serialization
    /// </summary>
    public class DateTimeUtcConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// Reads and converts JSON to a DateTime value in UTC.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The DateTime value in UTC, or DateTime.MinValue if parsing fails.</returns>
        public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.String &&
                DateTime.TryParse(reader.GetString(), out var dateTime))
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Writes a DateTime value to JSON in UTC format.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The DateTime value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
        {
            var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
            writer.WriteStringValue(utcValue.ToString("O"));
        }
    }

    /// <summary>
    /// Base class for models that support property change notifications
    /// </summary>
    public abstract class ObservableModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">Name of the property (automatically determined).</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">Names of the properties that changed.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Represents a single message in the AI assistant chat history.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>User, Assistant, or System.</summary>
        public string Role { get; set; } = "User";

        /// <summary>The message content.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>When the message was created.</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Whether this message is from the user.</summary>
        [JsonIgnore]
        public bool IsUser => string.Equals(Role, "User", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Represents the detected OpenCode execution runtime and its available models.
    /// </summary>
    public class DetectedRuntime : ObservableModel
    {
        private string _name = string.Empty;
        private string _command = string.Empty;
        private List<string> _models = new();
        private bool? _isAvailableCache;

        /// <summary>
        /// Gets or sets the runtime name. OpenCode is the only supported runtime.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the command path for the OpenCode executable.
        /// </summary>
        [JsonPropertyName("command")]
        public string Command
        {
            get => _command;
            set
            {
                if (SetProperty(ref _command, value))
                {
                    // Invalidate cache when command changes
                    _isAvailableCache = null;
                    OnPropertyChanged(nameof(IsAvailable));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of models surfaced by OpenCode for this runtime.
        /// </summary>
        [JsonPropertyName("models")]
        public List<string> Models
        {
            get => _models;
            set => SetProperty(ref _models, value);
        }

        /// <summary>
        /// Indicates if the runtime is available on the system (cached, lazy-initialized)
        /// </summary>
        [JsonIgnore]
        public bool IsAvailable => _isAvailableCache ??= CheckAvailability();

        /// <summary>
        /// Refreshes the availability check (call when file system may have changed)
        /// </summary>
        public void RefreshAvailability()
        {
            _isAvailableCache = null;
            OnPropertyChanged(nameof(IsAvailable));
            OnPropertyChanged(nameof(DisplayName));
        }

        private bool CheckAvailability()
        {
            return !string.IsNullOrEmpty(_command) && File.Exists(_command);
        }

        /// <summary>
        /// Display name with availability indicator
        /// </summary>
        [JsonIgnore]
        public string DisplayName => $"{Name} {(IsAvailable ? "✓" : "✗")}";

        /// <summary>
        /// Default model for this runtime (first available or "auto")
        /// </summary>
        [JsonIgnore]
        public string DefaultModel => Models.Count > 0 ? Models[0] : "auto";
    }

    /// <summary>
    /// Configuration for a specific role (create, improve, test, orchestrate, judge).
    /// </summary>
    public class RoleConfiguration : ObservableModel
    {
        private string _runtime = string.Empty;
        private string _model = "auto";

        /// <summary>
        /// Gets or sets the execution runtime for this role. OpenCode is the only supported value.
        /// </summary>
        [JsonPropertyName("runtime")]
        public string Runtime
        {
            get => _runtime;
            set
            {
                if (SetProperty(ref _runtime, value))
                    OnPropertyChanged(nameof(Label));
            }
        }

        /// <summary>
        /// Gets or sets the model to use for this role.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        /// <summary>
        /// Human-readable label for the role
        /// </summary>
        [JsonIgnore]
        public string Label => RoleLabels.GetLabel(_runtime);
    }

    /// <summary>
    /// Static helper for role labels (thread-safe)
    /// </summary>
    public static class RoleLabels
    {
        // Thread-safe dictionary for concurrent access
        private static readonly ConcurrentDictionary<string, string> Labels = new(new Dictionary<string, string>
        {
            ["create"] = "Creating Skills",
            ["improve"] = "Improving Skills",
            ["test"] = "Testing / Benchmarking / Evaluating",
            ["orchestrate"] = "Meta Management",
            ["judge"] = "LLM Judge"
        });

        /// <summary>
        /// Gets the human-readable label for the specified role.
        /// </summary>
        /// <param name="role">The role identifier.</param>
        /// <returns>The human-readable label, or the role identifier if no label is defined.</returns>
        public static string GetLabel(string role) => Labels.TryGetValue(role, out var label) ? label : role;

        /// <summary>
        /// Gets all available role labels as a read-only dictionary.
        /// </summary>
        public static IReadOnlyDictionary<string, string> All => Labels;

        /// <summary>
        /// Thread-safe method to add or update a label.
        /// </summary>
        /// <param name="role">The role identifier.</param>
        /// <param name="label">The human-readable label.</param>
        public static void AddOrUpdate(string role, string label) => Labels[role] = label;
    }

    /// <summary>
    /// Application configuration stored in .meta-skill-studio/config.json.
    /// </summary>
    public class AppConfiguration : ObservableModel
    {
        private int _version = 1;
        private DateTime _createdAtUtc = DateTime.UtcNow;
        private DateTime _lastUpdatedUtc = DateTime.UtcNow;
        private List<DetectedRuntime> _detectedRuntimes = new();
        private Dictionary<string, RoleConfiguration> _roles = new();

        /// <summary>
        /// Gets or sets the configuration format version.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        /// <summary>
        /// Gets or sets the UTC timestamp when the configuration was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime CreatedAtUtc
        {
            get => _createdAtUtc;
            set => SetProperty(ref _createdAtUtc, value);
        }

        /// <summary>
        /// Gets or sets the UTC timestamp when the configuration was last updated.
        /// </summary>
        [JsonPropertyName("last_updated")]
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime LastUpdatedUtc
        {
            get => _lastUpdatedUtc;
            set => SetProperty(ref _lastUpdatedUtc, value);
        }

        /// <summary>
        /// Gets or sets the list of detected AI CLI runtimes.
        /// </summary>
        [JsonPropertyName("detected_runtimes")]
        public List<DetectedRuntime> DetectedRuntimes
        {
            get => _detectedRuntimes;
            set => SetProperty(ref _detectedRuntimes, value);
        }

        /// <summary>
        /// Gets or sets the role configurations.
        /// </summary>
        [JsonPropertyName("roles")]
        public Dictionary<string, RoleConfiguration> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        /// <summary>
        /// Default roles with empty configuration
        /// </summary>
        public static Dictionary<string, RoleConfiguration> GetDefaultRoles()
        {
            return new Dictionary<string, RoleConfiguration>
            {
                ["create"] = new RoleConfiguration(),
                ["improve"] = new RoleConfiguration(),
                ["test"] = new RoleConfiguration(),
                ["orchestrate"] = new RoleConfiguration(),
                ["judge"] = new RoleConfiguration()
            };
        }

        /// <summary>
        /// Updates the LastUpdatedUtc timestamp to current UTC time
        /// </summary>
        public void Touch() => LastUpdatedUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Information about a skill package
    /// </summary>
    public class SkillInfo : ObservableModel
    {
        private string _name = string.Empty;
        private string _directoryPath = string.Empty;
        private string? _description;
        private bool _hasSkillMd;
        private bool _hasEvals;
        private string? _cachedDisplayName;
        private string? _lastDescriptionForCache;

        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the directory path where the skill is located.
        /// </summary>
        public string DirectoryPath
        {
            get => _directoryPath;
            set => SetProperty(ref _directoryPath, value);
        }

        /// <summary>
        /// Gets or sets the description of the skill from SKILL.md.
        /// </summary>
        public string? Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    // Invalidate display name cache when description changes
                    if (_lastDescriptionForCache != value)
                    {
                        _cachedDisplayName = null;
                        OnPropertyChanged(nameof(DisplayName));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the skill has a SKILL.md file.
        /// </summary>
        public bool HasSkillMd
        {
            get => _hasSkillMd;
            set => SetProperty(ref _hasSkillMd, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the skill has evaluation cases.
        /// </summary>
        public bool HasEvals
        {
            get => _hasEvals;
            set => SetProperty(ref _hasEvals, value);
        }

        /// <summary>
        /// Display name for UI (name + description preview) with caching
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                // Return cached value if description hasn't changed
                if (_cachedDisplayName != null && _lastDescriptionForCache == _description)
                    return _cachedDisplayName;

                _lastDescriptionForCache = _description;
                _cachedDisplayName = ToFriendlyName(_name);
                return _cachedDisplayName;
            }
        }

        private static string ToFriendlyName(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return folderName;
            var words = folderName.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i][1..];
            }
            return string.Join(' ', words);
        }

        /// <summary>
        /// Invalidates the display name cache (call if description was modified externally)
        /// </summary>
        public void InvalidateDisplayNameCache()
        {
            _cachedDisplayName = null;
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    /// <summary>
    /// Summary information about a visible skill-library surface.
    /// </summary>
    public class LibrarySurfaceInfo : ObservableModel
    {
        private string _name = string.Empty;
        private string _relativePath = string.Empty;
        private string _description = string.Empty;
        private int _directoryCount;
        private int _skillCount;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string RelativePath
        {
            get => _relativePath;
            set
            {
                if (SetProperty(ref _relativePath, value))
                {
                    OnPropertyChanged(nameof(DirectorySummary));
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int DirectoryCount
        {
            get => _directoryCount;
            set
            {
                if (SetProperty(ref _directoryCount, value))
                {
                    OnPropertyChanged(nameof(DirectorySummary));
                }
            }
        }

        public int SkillCount
        {
            get => _skillCount;
            set => SetProperty(ref _skillCount, value);
        }

        [JsonIgnore]
        public string DirectorySummary => $"{DirectoryCount} folders - {RelativePath}";
    }

    /// <summary>
    /// A single onboarding or readiness item for the studio dashboard.
    /// </summary>
    public class ChecklistItem : ObservableModel
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private bool _isComplete;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                if (SetProperty(ref _isComplete, value))
                {
                    OnPropertyChanged(nameof(StatusGlyph));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        [JsonIgnore]
        public string StatusGlyph => IsComplete ? "✓" : "•";

        [JsonIgnore]
        public string StatusText => IsComplete ? "Ready" : "Needs attention";
    }

    /// <summary>
    /// A documentation or help resource that can be opened from the studio dashboard.
    /// </summary>
    public class HelpResourceInfo : ObservableModel
    {
        private string _title = string.Empty;
        private string _relativePath = string.Empty;
        private string _description = string.Empty;
        private string _fullPath = string.Empty;
        private bool _isAvailable;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string RelativePath
        {
            get => _relativePath;
            set => SetProperty(ref _relativePath, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (SetProperty(ref _isAvailable, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        [JsonIgnore]
        public string StatusText => IsAvailable ? "Available" : "Missing";
    }

    /// <summary>
    /// Target library for skill creation
    /// </summary>
    public enum TargetLibrary
    {
        /// <summary>
        /// Unverified library for new or experimental skills.
        /// </summary>
        LibraryUnverified,

        /// <summary>
        /// Workbench library for skills that have passed initial validation.
        /// </summary>
        LibraryWorkbench,

        /// <summary>
        /// Verified library for production-ready skills.
        /// </summary>
        Library
    }

    /// <summary>
    /// Pages available in the studio navigation rail.
    /// </summary>
    public enum StudioPage
    {
        Dashboard,
        Library,
        Create,
        Improve,
        Test,
        Automation,
        Import,
        Manage,
        Analytics,
        Settings
    }

    /// <summary>
    /// A skill in a library tier with category context.
    /// </summary>
    public class LibrarySkillEntry
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplay { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public TargetLibrary Tier { get; set; }
        public bool HasSkillMd { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// A category within a library tier.
    /// </summary>
    public class LibraryCategory
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int SkillCount { get; set; }
        public TargetLibrary Tier { get; set; }
    }

    /// <summary>
    /// Provider authentication and availability information for Settings.
    /// </summary>
    public class ProviderStatusInfo : ObservableModel
    {
        private string _name = string.Empty;
        private bool _authenticated;
        private int _modelCount;
        private string _raw = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(StatusLabel));
                }
            }
        }

        public bool Authenticated
        {
            get => _authenticated;
            set
            {
                if (SetProperty(ref _authenticated, value))
                {
                    OnPropertyChanged(nameof(StatusLabel));
                }
            }
        }

        public int ModelCount
        {
            get => _modelCount;
            set => SetProperty(ref _modelCount, value);
        }

        public string Raw
        {
            get => _raw;
            set => SetProperty(ref _raw, value);
        }

        [JsonIgnore]
        public string StatusLabel => Authenticated ? "Connected" : "Needs sign-in";
    }

    /// <summary>
    /// Runtime model surfaced in Settings and Assistant.
    /// </summary>
    public class RuntimeModelInfo : ObservableModel
    {
        private string _runtime = string.Empty;
        private string _provider = string.Empty;
        private string _model = string.Empty;
        private bool _recommended;

        public string Runtime
        {
            get => _runtime;
            set => SetProperty(ref _runtime, value);
        }

        public string Provider
        {
            get => _provider;
            set => SetProperty(ref _provider, value);
        }

        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public bool Recommended
        {
            get => _recommended;
            set => SetProperty(ref _recommended, value);
        }

        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(Provider) ? Model : $"{Provider} / {Model}";
    }

    /// <summary>
    /// A single analytics metric surfaced in the inline dashboard.
    /// </summary>
    public class AnalyticsMetricInfo : ObservableModel
    {
        private string _label = string.Empty;
        private string _value = string.Empty;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    /// <summary>
    /// Result from a Python command execution
    /// </summary>
    public class RunResult : ObservableModel
    {
        private string _action = string.Empty;
        private int _exitCode;
        private string _stdout = string.Empty;
        private string _stderr = string.Empty;
        private double _durationSeconds;
        private DateTime _startedAtUtc;
        private DateTime _endedAtUtc;
        private Dictionary<string, object> _input = new();
        private Dictionary<string, object> _artifacts = new();

        /// <summary>
        /// Gets or sets the action that was executed.
        /// </summary>
        public string Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        /// <summary>
        /// Gets or sets the exit code from the process.
        /// </summary>
        public int ExitCode
        {
            get => _exitCode;
            set => SetProperty(ref _exitCode, value);
        }

        /// <summary>
        /// Gets or sets the standard output from the process.
        /// </summary>
        public string Stdout
        {
            get => _stdout;
            set => SetProperty(ref _stdout, value);
        }

        /// <summary>
        /// Gets or sets the standard error output from the process.
        /// </summary>
        public string Stderr
        {
            get => _stderr;
            set
            {
                if (SetProperty(ref _stderr, value))
                    OnPropertyChanged(nameof(CombinedOutput));
            }
        }

        /// <summary>
        /// Gets or sets the duration of the execution in seconds.
        /// </summary>
        public double DurationSeconds
        {
            get => _durationSeconds;
            set => SetProperty(ref _durationSeconds, value);
        }

        /// <summary>
        /// Gets or sets the UTC timestamp when execution started.
        /// </summary>
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime StartedAtUtc
        {
            get => _startedAtUtc;
            set => SetProperty(ref _startedAtUtc, value);
        }

        /// <summary>
        /// Gets or sets the UTC timestamp when execution ended.
        /// </summary>
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime EndedAtUtc
        {
            get => _endedAtUtc;
            set => SetProperty(ref _endedAtUtc, value);
        }

        /// <summary>
        /// Gets or sets the input parameters for the execution.
        /// </summary>
        public Dictionary<string, object> Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        /// <summary>
        /// Gets or sets the artifacts produced by the execution.
        /// </summary>
        public Dictionary<string, object> Artifacts
        {
            get => _artifacts;
            set => SetProperty(ref _artifacts, value);
        }

        /// <summary>
        /// Indicates if the command succeeded
        /// </summary>
        [JsonIgnore]
        public bool IsSuccess => _exitCode == 0;

        /// <summary>
        /// Status text for display (SUCCESS or FAILED)
        /// </summary>
        [JsonIgnore]
        public string StatusText => _exitCode == 0 ? "SUCCESS" : $"FAILED ({_exitCode})";

        /// <summary>
        /// Status badge background color
        /// </summary>
        [JsonIgnore]
        public string StatusColorClass => _exitCode == 0 ? "green" : "red";

        /// <summary>
        /// Combined output for display
        /// </summary>
        [JsonIgnore]
        public string CombinedOutput
        {
            get
            {
                var output = _stdout;
                if (!string.IsNullOrEmpty(_stderr))
                {
                    output += "\n\n[STDERR]\n" + _stderr;
                }
                return output;
            }
        }
    }

    /// <summary>
    /// Judge evaluation result parsed from run output
    /// </summary>
    public class JudgeResult : ObservableModel
    {
        private int? _qualityScore;
        private string? _routingNotes;
        private string? _behaviorNotes;
        private List<string>? _priorityFixes;

        /// <summary>
        /// Gets or sets the quality score (0-100) assigned by the judge.
        /// </summary>
        public int? QualityScore
        {
            get => _qualityScore;
            set
            {
                if (SetProperty(ref _qualityScore, value))
                    OnPropertyChanged(nameof(ScoreDisplay), nameof(ScoreColorClass));
            }
        }

        /// <summary>
        /// Gets or sets the routing quality notes from the judge evaluation.
        /// </summary>
        public string? RoutingNotes
        {
            get => _routingNotes;
            set => SetProperty(ref _routingNotes, value);
        }

        /// <summary>
        /// Gets or sets the behavior quality notes from the judge evaluation.
        /// </summary>
        public string? BehaviorNotes
        {
            get => _behaviorNotes;
            set => SetProperty(ref _behaviorNotes, value);
        }

        /// <summary>
        /// Gets or sets the list of highest priority fixes recommended by the judge.
        /// </summary>
        public List<string>? PriorityFixes
        {
            get => _priorityFixes;
            set => SetProperty(ref _priorityFixes, value);
        }

        /// <summary>
        /// Quality score formatted for display
        /// </summary>
        [JsonIgnore]
        public string ScoreDisplay => _qualityScore.HasValue ? $"{_qualityScore}/100" : "N/A";

        /// <summary>
        /// Color indicator for the score
        /// </summary>
        [JsonIgnore]
        public string ScoreColorClass
        {
            get
            {
                if (!_qualityScore.HasValue) return "gray";
                return _qualityScore.Value switch
                {
                    >= 80 => "green",
                    >= 60 => "yellow",
                    _ => "red"
                };
            }
        }

        /// <summary>
        /// Quality label based on score
        /// </summary>
        [JsonIgnore]
        public string QualityLabel
        {
            get
            {
                if (!_qualityScore.HasValue) return "No Score";
                return _qualityScore.Value switch
                {
                    >= 80 => "Excellent Quality",
                    >= 60 => "Good Quality",
                    _ => "Needs Improvement"
                };
            }
        }
    }

    /// <summary>
    /// Analytics data for a skill over time
    /// </summary>
    public class SkillAnalytics : ObservableModel
    {
        private string _skillName = string.Empty;
        private int _totalRuns;
        private int _successfulRuns;
        private int _failedRuns;
        private List<RunMetric> _runHistory = new();
        private DateTime? _lastRunAtUtc;
        private double? _averageQualityScore;
        private TrendDirection _trend = TrendDirection.Stable;

        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        public string SkillName
        {
            get => _skillName;
            set => SetProperty(ref _skillName, value);
        }

        /// <summary>
        /// Gets or sets the total number of runs for this skill.
        /// </summary>
        public int TotalRuns
        {
            get => _totalRuns;
            set
            {
                if (SetProperty(ref _totalRuns, value))
                {
                    SynchronizeFailedRuns();
                    OnPropertyChanged(nameof(SuccessRate));
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of successful runs for this skill.
        /// </summary>
        public int SuccessfulRuns
        {
            get => _successfulRuns;
            set
            {
                if (SetProperty(ref _successfulRuns, value))
                {
                    SynchronizeFailedRuns();
                    OnPropertyChanged(nameof(SuccessRate));
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of failed runs for this skill.
        /// </summary>
        public int FailedRuns
        {
            get => _failedRuns;
            set => SetProperty(ref _failedRuns, value);
        }

        private void SynchronizeFailedRuns()
        {
            var calculatedFailedRuns = Math.Max(0, _totalRuns - _successfulRuns);
            if (_failedRuns != calculatedFailedRuns)
            {
                _failedRuns = calculatedFailedRuns;
                OnPropertyChanged(nameof(FailedRuns));
            }
        }

        /// <summary>
        /// Gets the success rate as a percentage.
        /// </summary>
        [JsonIgnore]
        public double SuccessRate => _totalRuns > 0 ? (double)_successfulRuns / _totalRuns * 100 : 0;

        /// <summary>
        /// Gets or sets the history of run metrics.
        /// </summary>
        public List<RunMetric> RunHistory
        {
            get => _runHistory;
            set => SetProperty(ref _runHistory, value);
        }

        /// <summary>
        /// Gets or sets the UTC timestamp of the last run.
        /// </summary>
        public DateTime? LastRunAtUtc
        {
            get => _lastRunAtUtc;
            set => SetProperty(ref _lastRunAtUtc, value);
        }

        /// <summary>
        /// Gets or sets the average quality score across runs.
        /// </summary>
        public double? AverageQualityScore
        {
            get => _averageQualityScore;
            set => SetProperty(ref _averageQualityScore, value);
        }

        /// <summary>
        /// Gets or sets the trend direction of the skill's performance.
        /// </summary>
        public TrendDirection Trend
        {
            get => _trend;
            set => SetProperty(ref _trend, value);
        }
    }

    /// <summary>
    /// Represents a single run metric for analytics.
    /// </summary>
    public class RunMetric : ObservableModel
    {
        private DateTime _timestampUtc;
        private string _action = string.Empty;
        private bool _isSuccess;
        private double _durationSeconds;
        private int? _qualityScore;

        /// <summary>
        /// Gets or sets the UTC timestamp of the run.
        /// </summary>
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime TimestampUtc
        {
            get => _timestampUtc;
            set => SetProperty(ref _timestampUtc, value);
        }

        /// <summary>
        /// Gets or sets the action that was executed.
        /// </summary>
        public string Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the run was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        /// <summary>
        /// Gets or sets the duration of the run in seconds.
        /// </summary>
        public double DurationSeconds
        {
            get => _durationSeconds;
            set => SetProperty(ref _durationSeconds, value);
        }

        /// <summary>
        /// Gets or sets the quality score for this run, if available.
        /// </summary>
        public int? QualityScore
        {
            get => _qualityScore;
            set => SetProperty(ref _qualityScore, value);
        }
    }

    /// <summary>
    /// Defines the trend direction for skill performance over time.
    /// </summary>
    public enum TrendDirection
    {
        /// <summary>
        /// Performance is improving over time.
        /// </summary>
        Improving,

        /// <summary>
        /// Performance is stable over time.
        /// </summary>
        Stable,

        /// <summary>
        /// Performance is declining over time.
        /// </summary>
        Declining
    }

    /// <summary>
    /// Information about a saved run file for display in the run history
    /// </summary>
    public class RunInfo : ObservableModel
    {
        private string _filePath = string.Empty;
        private string _displayName = string.Empty;
        private DateTime _timestamp;

        /// <summary>
        /// Gets or sets the file path of the saved run.
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// Gets or sets the display name for the run.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Gets or sets the timestamp when the run was saved.
        /// </summary>
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }
    }
}
