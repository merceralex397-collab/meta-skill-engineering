using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MetaSkillStudio.Extensions
{
    /// <summary>
    /// Extension methods for Task with safe fire-and-forget and error handling.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Safely fire and forget a task, logging any exceptions that occur.
        /// NEVER discard a Task without exception handling - this prevents unobserved exceptions.
        /// </summary>
        public static void SafeFireAndForget(this Task task, string operationName = "Background task")
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    // Log the exception - use Debug.WriteLine for WPF apps
                    // In production, replace with proper logging (ILogger, Serilog, etc.)
                    foreach (var ex in t.Exception.Flatten().InnerExceptions)
                    {
                        Debug.WriteLine($"[{operationName}] Error: {ex.Message}");
                        Debug.WriteLine($"[{operationName}] Stack: {ex.StackTrace}");
                    }
                }
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// Safely fire and forget a task with a custom error handler.
        /// </summary>
        public static void SafeFireAndForget(this Task task, Action<Exception> errorHandler)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    foreach (var ex in t.Exception.Flatten().InnerExceptions)
                    {
                        errorHandler(ex);
                    }
                }
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// Safely fire and forget a task with an async error handler.
        /// </summary>
        public static void SafeFireAndForget(this Task task, Func<Exception, Task> errorHandler)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            _ = task.ContinueWith(async t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    foreach (var ex in t.Exception.Flatten().InnerExceptions)
                    {
                        await errorHandler(ex);
                    }
                }
            }, TaskScheduler.Default);
        }
    }
}
