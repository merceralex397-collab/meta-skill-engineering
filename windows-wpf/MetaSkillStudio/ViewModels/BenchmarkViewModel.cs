using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Commands;

namespace MetaSkillStudio.ViewModels
{
    /// <summary>
    /// ViewModel for the BenchmarkDialog, handling benchmark creation with progress tracking.
    /// </summary>
    public class BenchmarkViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _skillName = string.Empty;
        private string _benchmarkGoal = string.Empty;
        private int _caseCount = 8;
        private bool _isBusy = false;
        private int _progress = 0;
        private string _progressText = "Generating benchmark cases...";
        private bool? _dialogResult = null;

        /// <summary>
        /// Initializes a new instance of the BenchmarkViewModel class.
        /// </summary>
        public BenchmarkViewModel()
        {
            CreateBenchmarksCommand = new RelayCommand(
                async () => await CreateBenchmarksAsync(),
                () => !IsBusy && IsValid);
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the skill to create benchmarks for.
        /// </summary>
        [Required(ErrorMessage = "Skill name is required.")]
        public string SkillName
        {
            get => _skillName;
            set
            {
                if (SetProperty(ref _skillName, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                    InvalidateCommands();
                }
            }
        }

        /// <summary>
        /// Gets or sets the benchmark goal describing what to test.
        /// </summary>
        [Required(ErrorMessage = "Benchmark goal is required.")]
        public string BenchmarkGoal
        {
            get => _benchmarkGoal;
            set
            {
                if (SetProperty(ref _benchmarkGoal, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                    InvalidateCommands();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of benchmark cases to generate.
        /// </summary>
        public int CaseCount
        {
            get => _caseCount;
            set => SetProperty(ref _caseCount, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// When true, the UI shows a busy overlay and commands are disabled.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    InvalidateCommands();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current progress percentage (0-100).
        /// </summary>
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        /// <summary>
        /// Gets or sets the progress status text displayed during benchmark creation.
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        /// <summary>
        /// Gets or sets the dialog result indicating how the dialog was closed.
        /// Null if not closed, true if create was clicked, false if cancelled.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        /// <summary>
        /// Gets a value indicating whether all required fields are valid.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(SkillName) && !string.IsNullOrWhiteSpace(BenchmarkGoal);

        /// <summary>
        /// Gets the command to create benchmarks.
        /// </summary>
        public ICommand CreateBenchmarksCommand { get; }

        #endregion

        #region Command Execution

        /// <summary>
        /// Executes the benchmark creation process with progress tracking.
        /// </summary>
        private async Task CreateBenchmarksAsync()
        {
            if (!IsValid)
            {
                return;
            }

            IsBusy = true;
            Progress = 0;
            ProgressText = "Initializing benchmark creation...";

            try
            {
                // Simulate progress for benchmark generation
                await UpdateProgressAsync(10, "Analyzing skill requirements...");
                await Task.Delay(200);

                await UpdateProgressAsync(30, "Generating test scenarios...");
                await Task.Delay(300);

                await UpdateProgressAsync(50, "Creating trigger-positive cases...");
                await Task.Delay(250);

                await UpdateProgressAsync(70, "Creating trigger-negative cases...");
                await Task.Delay(250);

                await UpdateProgressAsync(90, "Validating benchmark cases...");
                await Task.Delay(200);

                await UpdateProgressAsync(100, "Benchmark creation complete!");
                await Task.Delay(100);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ProgressText = $"Error: {ex.Message}";
                DialogResult = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Updates the progress with the specified percentage and status text.
        /// </summary>
        private async Task UpdateProgressAsync(int progress, string statusText)
        {
            Progress = progress;
            ProgressText = statusText;
            // Allow UI to update
            await Task.Delay(1);
        }

        /// <summary>
        /// Triggers command requery to update CanExecute state.
        /// </summary>
        private void InvalidateCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region IDataErrorInfo Implementation

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error => string.Empty;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(SkillName) && string.IsNullOrWhiteSpace(SkillName))
                {
                    return "Please enter a skill name.";
                }
                if (columnName == nameof(BenchmarkGoal) && string.IsNullOrWhiteSpace(BenchmarkGoal))
                {
                    return "Please enter a benchmark goal.";
                }
                return string.Empty;
            }
        }

        #endregion

        #region INotifyPropertyChanged

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
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
