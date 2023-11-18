#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/ReenterableAwaiter{T}.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities.Threading;

public class ReenterableAwaiter<TResult>(bool initialSignal, TResult resultInitialSignalIsTrue) : INotifyCompletion
{
    private Action? _continuation;
    private bool _continueOnCapturedContext = true; // whether the continuation should be posted to the captured SynchronizationContext
    private Exception? _exception;
    private TResult? _result = resultInitialSignalIsTrue;

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

    public TResult GetResult()
    {
        if (_exception is not null)
        {
            throw _exception;
        }

        return _result!;
    }

    // Signals the awaiter to complete successfully
    public void SetResult(TResult result)
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            _result = result;
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

    public ReenterableAwaiter<TResult> ConfigureAwait(bool continueOnCapturedContext)
    {
        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    public ReenterableAwaiter<TResult> GetAwaiter()
    {
        return this;
    }
}