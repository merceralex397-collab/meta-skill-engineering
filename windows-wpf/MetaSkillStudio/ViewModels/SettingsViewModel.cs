using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Commands;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings dialog, managing runtime and model configuration
    /// for all operational roles (Create, Improve, Test, Orchestrate, Judge).
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IPythonRuntimeService _pythonService;
        private ObservableCollection<DetectedRuntime> _availableRuntimes = new();

        // Role configuration backing fields - Create role
        private DetectedRuntime? _createSelectedRuntime;
        private string _createSelectedModel = "auto";
        private ObservableCollection<string> _createAvailableModels = new() { "auto" };

        // Role configuration backing fields - Improve role
        private DetectedRuntime? _improveSelectedRuntime;
        private string _improveSelectedModel = "auto";
        private ObservableCollection<string> _improveAvailableModels = new() { "auto" };

        // Role configuration backing fields - Test role
        private DetectedRuntime? _testSelectedRuntime;
        private string _testSelectedModel = "auto";
        private ObservableCollection<string> _testAvailableModels = new() { "auto" };

        // Role configuration backing fields - Orchestrate role
        private DetectedRuntime? _orchestrateSelectedRuntime;
        private string _orchestrateSelectedModel = "auto";
        private ObservableCollection<string> _orchestrateAvailableModels = new() { "auto" };

        // Role configuration backing fields - Judge role
        private DetectedRuntime? _judgeSelectedRuntime;
        private string _judgeSelectedModel = "auto";
        private ObservableCollection<string> _judgeAvailableModels = new() { "auto" };

        private string _runtimeStatusMessage = "Checking AI runtime availability...";

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        /// <param name="pythonService">The Python runtime service for detecting and configuring runtimes.</param>
        /// <exception cref="ArgumentNullException">Thrown when pythonService is null.</exception>
        public SettingsViewModel(IPythonRuntimeService pythonService)
        {
            _pythonService = pythonService ?? throw new ArgumentNullException(nameof(pythonService));

            DetectRuntimesCommand = new RelayCommand(async () => await DetectRuntimesAsync());
        }

        #region Properties

        /// <summary>
        /// Gets or sets the detected OpenCode runtime surfaced to the settings UI.
        /// </summary>
        public ObservableCollection<DetectedRuntime> AvailableRuntimes
        {
            get => _availableRuntimes;
            set => SetProperty(ref _availableRuntimes, value);
        }

        /// <summary>
        /// Gets or sets the status message for runtime detection operations.
        /// </summary>
        public string RuntimeStatusMessage
        {
            get => _runtimeStatusMessage;
            set => SetProperty(ref _runtimeStatusMessage, value);
        }

        #region Create Role Properties

        /// <summary>
        /// Gets or sets the selected runtime for the Create role.
        /// </summary>
        public DetectedRuntime? CreateSelectedRuntime
        {
            get => _createSelectedRuntime;
            set
            {
                if (SetProperty(ref _createSelectedRuntime, value) && value != null)
                {
                    CreateAvailableModels = new ObservableCollection<string>(
                        value.Models.Any() ? value.Models : new[] { "auto" });
                    CreateSelectedModel = value.DefaultModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected model for the Create role.
        /// </summary>
        public string CreateSelectedModel
        {
            get => _createSelectedModel;
            set => SetProperty(ref _createSelectedModel, value);
        }

        /// <summary>
        /// Gets or sets the collection of available models for the Create role's selected runtime.
        /// </summary>
        public ObservableCollection<string> CreateAvailableModels
        {
            get => _createAvailableModels;
            set => SetProperty(ref _createAvailableModels, value);
        }

        #endregion

        #region Improve Role Properties

        /// <summary>
        /// Gets or sets the selected runtime for the Improve role.
        /// </summary>
        public DetectedRuntime? ImproveSelectedRuntime
        {
            get => _improveSelectedRuntime;
            set
            {
                if (SetProperty(ref _improveSelectedRuntime, value) && value != null)
                {
                    ImproveAvailableModels = new ObservableCollection<string>(
                        value.Models.Any() ? value.Models : new[] { "auto" });
                    ImproveSelectedModel = value.DefaultModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected model for the Improve role.
        /// </summary>
        public string ImproveSelectedModel
        {
            get => _improveSelectedModel;
            set => SetProperty(ref _improveSelectedModel, value);
        }

        /// <summary>
        /// Gets or sets the collection of available models for the Improve role's selected runtime.
        /// </summary>
        public ObservableCollection<string> ImproveAvailableModels
        {
            get => _improveAvailableModels;
            set => SetProperty(ref _improveAvailableModels, value);
        }

        #endregion

        #region Test Role Properties

        /// <summary>
        /// Gets or sets the selected runtime for the Test role.
        /// </summary>
        public DetectedRuntime? TestSelectedRuntime
        {
            get => _testSelectedRuntime;
            set
            {
                if (SetProperty(ref _testSelectedRuntime, value) && value != null)
                {
                    TestAvailableModels = new ObservableCollection<string>(
                        value.Models.Any() ? value.Models : new[] { "auto" });
                    TestSelectedModel = value.DefaultModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected model for the Test role.
        /// </summary>
        public string TestSelectedModel
        {
            get => _testSelectedModel;
            set => SetProperty(ref _testSelectedModel, value);
        }

        /// <summary>
        /// Gets or sets the collection of available models for the Test role's selected runtime.
        /// </summary>
        public ObservableCollection<string> TestAvailableModels
        {
            get => _testAvailableModels;
            set => SetProperty(ref _testAvailableModels, value);
        }

        #endregion

        #region Orchestrate Role Properties

        /// <summary>
        /// Gets or sets the selected runtime for the Orchestrate role.
        /// </summary>
        public DetectedRuntime? OrchestrateSelectedRuntime
        {
            get => _orchestrateSelectedRuntime;
            set
            {
                if (SetProperty(ref _orchestrateSelectedRuntime, value) && value != null)
                {
                    OrchestrateAvailableModels = new ObservableCollection<string>(
                        value.Models.Any() ? value.Models : new[] { "auto" });
                    OrchestrateSelectedModel = value.DefaultModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected model for the Orchestrate role.
        /// </summary>
        public string OrchestrateSelectedModel
        {
            get => _orchestrateSelectedModel;
            set => SetProperty(ref _orchestrateSelectedModel, value);
        }

        /// <summary>
        /// Gets or sets the collection of available models for the Orchestrate role's selected runtime.
        /// </summary>
        public ObservableCollection<string> OrchestrateAvailableModels
        {
            get => _orchestrateAvailableModels;
            set => SetProperty(ref _orchestrateAvailableModels, value);
        }

        #endregion

        #region Judge Role Properties

        /// <summary>
        /// Gets or sets the selected runtime for the Judge role.
        /// </summary>
        public DetectedRuntime? JudgeSelectedRuntime
        {
            get => _judgeSelectedRuntime;
            set
            {
                if (SetProperty(ref _judgeSelectedRuntime, value) && value != null)
                {
                    JudgeAvailableModels = new ObservableCollection<string>(
                        value.Models.Any() ? value.Models : new[] { "auto" });
                    JudgeSelectedModel = value.DefaultModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected model for the Judge role.
        /// </summary>
        public string JudgeSelectedModel
        {
            get => _judgeSelectedModel;
            set => SetProperty(ref _judgeSelectedModel, value);
        }

        /// <summary>
        /// Gets or sets the collection of available models for the Judge role's selected runtime.
        /// </summary>
        public ObservableCollection<string> JudgeAvailableModels
        {
            get => _judgeAvailableModels;
            set => SetProperty(ref _judgeAvailableModels, value);
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to re-scan OpenCode on the system.
        /// </summary>
        public ICommand DetectRuntimesCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the configuration asynchronously, detecting runtimes and applying saved settings.
        /// </summary>
        public async Task LoadConfigurationAsync()
        {
            try
            {
                RuntimeStatusMessage = "Checking AI runtime availability...";

                var runtimes = await _pythonService.DetectRuntimesAsync();
                AvailableRuntimes = new ObservableCollection<DetectedRuntime>(runtimes);

                var opencodeRuntime = runtimes.FirstOrDefault();
                if (opencodeRuntime?.IsAvailable == true)
                {
                    RuntimeStatusMessage = $"AI runtime detected. {opencodeRuntime.Models.Count} model option(s) available.";
                }
                else
                {
                    RuntimeStatusMessage = "AI runtime not detected. Install the repo-local SDK/runtime dependencies or expose `opencode` on PATH.";
                }

                // Load existing configuration if available
                var config = _pythonService.LoadConfiguration();
                if (config != null)
                {
                    ApplyConfiguration(config, runtimes);
                }
                else
                {
                    // Set defaults to the detected runtime
                    var defaultRuntime = runtimes.FirstOrDefault(r => r.IsAvailable);
                    if (defaultRuntime != null)
                    {
                        CreateSelectedRuntime = defaultRuntime;
                        ImproveSelectedRuntime = defaultRuntime;
                        TestSelectedRuntime = defaultRuntime;
                        OrchestrateSelectedRuntime = defaultRuntime;
                        JudgeSelectedRuntime = defaultRuntime;
                    }
                }
            }
            catch (Exception ex)
            {
                RuntimeStatusMessage = $"Error detecting AI runtime: {ex.Message}";
                throw;
            }
        }

        /// <summary>
        /// Detects runtimes and reloads the configuration.
        /// </summary>
        private async Task DetectRuntimesAsync()
        {
            RuntimeStatusMessage = "Re-scanning AI runtime...";
            await LoadConfigurationAsync();
        }

        /// <summary>
        /// Applies the loaded configuration to the ViewModel properties.
        /// </summary>
        /// <param name="config">The application configuration to apply.</param>
        /// <param name="runtimes">The list of detected runtimes (OpenCode only).</param>
        private void ApplyConfiguration(AppConfiguration config, List<DetectedRuntime> runtimes)
        {
            if (config.Roles.TryGetValue("create", out var createRole))
            {
                CreateSelectedRuntime = runtimes.FirstOrDefault(r => r.Name == createRole.Runtime) ?? runtimes.FirstOrDefault(r => r.IsAvailable);
                CreateSelectedModel = createRole.Model;
            }

            if (config.Roles.TryGetValue("improve", out var improveRole))
            {
                ImproveSelectedRuntime = runtimes.FirstOrDefault(r => r.Name == improveRole.Runtime) ?? runtimes.FirstOrDefault(r => r.IsAvailable);
                ImproveSelectedModel = improveRole.Model;
            }

            if (config.Roles.TryGetValue("test", out var testRole))
            {
                TestSelectedRuntime = runtimes.FirstOrDefault(r => r.Name == testRole.Runtime) ?? runtimes.FirstOrDefault(r => r.IsAvailable);
                TestSelectedModel = testRole.Model;
            }

            if (config.Roles.TryGetValue("orchestrate", out var orchestrateRole))
            {
                OrchestrateSelectedRuntime = runtimes.FirstOrDefault(r => r.Name == orchestrateRole.Runtime) ?? runtimes.FirstOrDefault(r => r.IsAvailable);
                OrchestrateSelectedModel = orchestrateRole.Model;
            }

            if (config.Roles.TryGetValue("judge", out var judgeRole))
            {
                JudgeSelectedRuntime = runtimes.FirstOrDefault(r => r.Name == judgeRole.Runtime) ?? runtimes.FirstOrDefault(r => r.IsAvailable);
                JudgeSelectedModel = judgeRole.Model;
            }
        }

        /// <summary>
        /// Saves the current configuration to persistent storage.
        /// </summary>
        /// <returns>The saved application configuration.</returns>
        public AppConfiguration SaveConfiguration()
        {
            var config = new AppConfiguration
            {
                DetectedRuntimes = AvailableRuntimes.ToList(),
                Roles = new Dictionary<string, RoleConfiguration>()
            };

            config.Roles["create"] = new RoleConfiguration
            {
                Runtime = "opencode",
                Model = CreateSelectedModel
            };

            config.Roles["improve"] = new RoleConfiguration
            {
                Runtime = "opencode",
                Model = ImproveSelectedModel
            };

            config.Roles["test"] = new RoleConfiguration
            {
                Runtime = "opencode",
                Model = TestSelectedModel
            };

            config.Roles["orchestrate"] = new RoleConfiguration
            {
                Runtime = "opencode",
                Model = OrchestrateSelectedModel
            };

            config.Roles["judge"] = new RoleConfiguration
            {
                Runtime = "opencode",
                Model = JudgeSelectedModel
            };

            _pythonService.SaveConfiguration(config);

            return config;
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The backing field reference.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property (auto-detected).</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        #endregion
    }
}
