using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MetaSkillStudio.Tests.Helpers
{
    /// <summary>
    /// Helper methods for testing async code.
    /// </summary>
    public static class AsyncTestHelper
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Runs an async method synchronously with timeout.
        /// </summary>
        public static void RunSync(Func<Task> asyncFunc, TimeSpan? timeout = null)
        {
            var task = asyncFunc();
            if (!task.Wait(timeout ?? DefaultTimeout))
            {
                throw new TimeoutException("Test operation timed out");
            }
        }

        /// <summary>
        /// Runs an async method synchronously with timeout and returns the result.
        /// </summary>
        public static T RunSync<T>(Func<Task<T>> asyncFunc, TimeSpan? timeout = null)
        {
            var task = asyncFunc();
            if (!task.Wait(timeout ?? DefaultTimeout))
            {
                throw new TimeoutException("Test operation timed out");
            }
            return task.Result;
        }

        /// <summary>
        /// Waits for a condition to become true with timeout and polling.
        /// </summary>
        public static async Task WaitForConditionAsync(Func<bool> condition, TimeSpan? timeout = null, TimeSpan? pollInterval = null)
        {
            var actualTimeout = timeout ?? DefaultTimeout;
            var actualPollInterval = pollInterval ?? TimeSpan.FromMilliseconds(100);
            var startTime = DateTime.UtcNow;

            while (!condition())
            {
                if (DateTime.UtcNow - startTime > actualTimeout)
                {
                    throw new TimeoutException("Condition was not met within the timeout");
                }
                await Task.Delay(actualPollInterval);
            }
        }

        /// <summary>
        /// Creates a task that completes after a specified delay (useful for simulating async operations).
        /// </summary>
        public static async Task<T> DelayedResult<T>(T result, int millisecondsDelay)
        {
            await Task.Delay(millisecondsDelay);
            return result;
        }

        /// <summary>
        /// Creates a cancellable task that completes when cancelled or timeout occurs.
        /// </summary>
        public static async Task<T> WithCancellation<T>(Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                var completed = await Task.WhenAny(task, tcs.Task);
                return await completed;
            }
        }

        /// <summary>
        /// Asserts that a task completes within a specified timeout.
        /// </summary>
        public static async Task AssertCompletesWithinAsync(Task task, TimeSpan timeout, string? message = null)
        {
            var completed = await Task.WhenAny(task, Task.Delay(timeout));
            Assert.True(completed == task, message ?? $"Task did not complete within {timeout}");
        }

        /// <summary>
        /// Asserts that an async operation throws an exception of the specified type.
        /// </summary>
        public static async Task<TException> AssertThrowsAsync<TException>(Func<Task> asyncFunc) where TException : Exception
        {
            try
            {
                await asyncFunc();
                Assert.Fail($"Expected exception of type {typeof(TException).Name} but no exception was thrown");
                return null!; // Unreachable
            }
            catch (TException ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Creates multiple tasks running in parallel and waits for all to complete.
        /// </summary>
        public static async Task WhenAllWithTimeoutAsync(IEnumerable<Task> tasks, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? DefaultTimeout;
            using var cts = new CancellationTokenSource(actualTimeout);
            
            try
            {
                await Task.WhenAll(tasks).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Tasks did not complete within {actualTimeout}");
            }
        }

        /// <summary>
        /// Repeatedly executes an action until it succeeds or max attempts reached.
        /// </summary>
        public static async Task<TResult> RetryAsync<TResult>(Func<Task<TResult>> operation, int maxAttempts = 3, TimeSpan? delayBetweenAttempts = null)
        {
            var delay = delayBetweenAttempts ?? TimeSpan.FromMilliseconds(100);
            Exception? lastException = null;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < maxAttempts - 1)
                    {
                        await Task.Delay(delay);
                    }
                }
            }

            throw new InvalidOperationException($"Operation failed after {maxAttempts} attempts", lastException);
        }
    }
}
