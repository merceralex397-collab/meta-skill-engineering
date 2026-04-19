using System;
using System.Threading.Tasks;

namespace MetaSkillStudio.Services.Interfaces
{
    /// <summary>
    /// Abstraction for UI thread dispatching operations.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Invokes an action on the UI thread synchronously.
        /// </summary>
        void Invoke(Action action);

        /// <summary>
        /// Invokes an action on the UI thread asynchronously.
        /// </summary>
        Task InvokeAsync(Action action);

        /// <summary>
        /// Invokes a function on the UI thread asynchronously and returns the result.
        /// </summary>
        Task<T> InvokeAsync<T>(Func<T> func);

        /// <summary>
        /// Checks if the current thread is the UI thread.
        /// </summary>
        bool CheckAccess();
    }
}
