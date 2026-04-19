using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IDispatcher for testing.
    /// Executes actions immediately on the current thread instead of dispatching.
    /// </summary>
    public class MockDispatcher : IDispatcher
    {
        // Tracking properties
        public int InvokeCallCount { get; private set; }
        public int InvokeAsyncCallCount { get; private set; }
        public int InvokeAsyncTCallCount { get; private set; }
        public int CheckAccessCallCount { get; private set; }

        // Configuration
        public bool AlwaysReturnsTrueForCheckAccess { get; set; } = true;
        public bool SimulateAsyncDelay { get; set; } = false;
        public int SimulatedDelayMilliseconds { get; set; } = 0;

        // Track invoked actions
        public List<Action> InvokedActions { get; } = new();
        public List<Func<object>> InvokedFunctions { get; } = new();

        public void Invoke(Action action)
        {
            InvokeCallCount++;
            InvokedActions.Add(action);
            action();
        }

        public Task InvokeAsync(Action action)
        {
            InvokeAsyncCallCount++;
            InvokedActions.Add(action);
            
            if (SimulateAsyncDelay && SimulatedDelayMilliseconds > 0)
            {
                return Task.Delay(SimulatedDelayMilliseconds).ContinueWith(_ => action());
            }

            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            InvokeAsyncTCallCount++;
            InvokedFunctions.Add(() => func()!);
            
            if (SimulateAsyncDelay && SimulatedDelayMilliseconds > 0)
            {
                return Task.Delay(SimulatedDelayMilliseconds).ContinueWith(_ => func());
            }

            return Task.FromResult(func());
        }

        public bool CheckAccess()
        {
            CheckAccessCallCount++;
            return AlwaysReturnsTrueForCheckAccess;
        }

        public void Reset()
        {
            InvokeCallCount = 0;
            InvokeAsyncCallCount = 0;
            InvokeAsyncTCallCount = 0;
            CheckAccessCallCount = 0;
            InvokedActions.Clear();
            InvokedFunctions.Clear();
        }
    }
}
