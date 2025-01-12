// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities.Threading;

public class ReenterableAwaiter(bool initialSignal) : INotifyCompletion
{
    private Action? _continuation;
    private bool _continueOnCapturedContext = true; // whether the continuation should be posted to the captured SynchronizationContext
    private Exception? _exception;

    public bool IsCompleted { get; set; } = initialSignal;

    public void OnCompleted(Action continuation)
    {
        // Stores the continuation
        // If your awaiter is intended to be used across multiple
        // task boundaries, you can use a thread-safe collection
        // to hold all the continuations
        _continuation = continuation;
    }

    public void Reset()
    {
        IsCompleted = false; // Set the awaiter to non-completed
        _continuation = null;
        _exception = null;
    }

    public void GetResult()
    {
        if (_exception is not null)
        {
            ThrowUtils.Throw(_exception);
        }
    }

    // Signals the awaiter to complete successfully
    public void SetResult()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            _continuation?.Invoke();
        }
    }

    // Signals the awaiter to complete unsuccessfully
    public void SetException(Exception exception)
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            _exception = exception;
            CompleteInternal();
        }
    }

    // Queue the continuation to SynchronizationContext.Current if _continueOnCapturedContext is true,
    // otherwise schedule it on the default TaskScheduler
    private void CompleteInternal()
    {
        if (_continuation is null)
        {
            return;
        }

        if (_continueOnCapturedContext && SynchronizationContext.Current is { } context)
        {
            context.Post(cont => (cont as Action)?.Invoke(), _continuation);
        }
        else
        {
            _ = Task.Factory.StartNew(_continuation, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }

    public ReenterableAwaiter ConfigureAwait(bool continueOnCapturedContext)
    {
        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    public ReenterableAwaiter GetAwaiter()
    {
        return this;
    }
}
