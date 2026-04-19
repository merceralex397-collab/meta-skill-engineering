using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Services
{
    /// <summary>
    /// Implementation of IDispatcher that wraps the WPF Application dispatcher.
    /// </summary>
    public class DispatcherService : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherService()
        {
            _dispatcher = System.Windows.Application.Current?.Dispatcher 
                ?? throw new InvalidOperationException("Application.Current is not available");
        }

        /// <summary>
        /// Invokes an action on the UI thread synchronously.
        /// </summary>
        public void Invoke(Action action)
        {
            if (_dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _dispatcher.Invoke(action);
            }
        }

        /// <summary>
        /// Invokes an action on the UI thread asynchronously.
        /// </summary>
        public Task InvokeAsync(Action action)
        {
            if (_dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                return _dispatcher.InvokeAsync(action).Task;
            }
        }

        /// <summary>
        /// Invokes a function on the UI thread asynchronously and returns the result.
        /// </summary>
        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            if (_dispatcher.CheckAccess())
            {
                return Task.FromResult(func());
            }
            else
            {
                return _dispatcher.InvokeAsync(func).Task;
            }
        }

        /// <summary>
        /// Checks if the current thread is the UI thread.
        /// </summary>
        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }
    }
}
