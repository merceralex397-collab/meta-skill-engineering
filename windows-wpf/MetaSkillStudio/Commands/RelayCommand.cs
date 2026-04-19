using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaSkillStudio.Extensions;

namespace MetaSkillStudio.Commands
{
    /// <summary>
    /// Relay command implementation for ICommand.
    /// Supports asynchronous execution with exception handling.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="canExecute">Optional function to determine if the command can execute. Defaults to always true.</param>
        /// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. Not used in this implementation.</param>
        /// <returns>true if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Executes the command asynchronously.
        /// SECURITY FIX: Converted async void to proper exception handling.
        /// NEVER let exceptions escape from async void methods - they crash the application.
        /// </summary>
        /// <param name="parameter">Data used by the command. Not used in this implementation.</param>
        public void Execute(object? parameter)
        {
            _execute().SafeFireAndForget(ex =>
            {
                // Route errors to Debug output - in production, use proper logging
                Debug.WriteLine($"[RelayCommand] Unhandled exception: {ex}");
            });
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">Data used by the command. Not used in this implementation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ExecuteAsync(object? parameter)
        {
            return _execute();
        }
    }
}
