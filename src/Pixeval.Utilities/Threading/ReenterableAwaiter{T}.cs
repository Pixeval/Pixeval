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

public class ReenterableAwaiter<TResult>(bool initialSignal, TResult resultInitialSignalIsTrue) : INotifyCompletion, IDisposable
{
    private Action? _continuation;
    private bool _continueOnCapturedContext = true; // whether the continuation should be posted to the captured SynchronizationContext
    private Exception? _exception;
    private TResult? _result = resultInitialSignalIsTrue;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public bool IsCompleted { get; set; } = initialSignal;

    public void OnCompleted(Action continuation)
    {
        // Stores the continuation
        // If your awaiter is intended to be used across multiple
        // task boundaries, you can use a thread-safe collection
        // to hold all the continuations
        _continuation = continuation;
    }

    public async Task ResetAsync()
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            IsCompleted = false; // Set the awaiter to non-completed
            _continuation = null;
            _exception = null;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public TResult GetResult()
    {
        return _exception is null ? _result! : ThrowUtils.Throw<TResult>(_exception);
    }

    /// <summary>
    /// Signals the awaiter to complete successfully
    /// </summary>
    /// <param name="result"></param>
    public async Task SetResultAsync(TResult result)
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            if (!IsCompleted)
            {
                IsCompleted = true;
                _result = result;
                _continuation?.Invoke();
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Signals the awaiter to complete unsuccessfully
    /// </summary>
    /// <param name="exception"></param>
    public void SetException(Exception exception)
    {
        try
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                _exception = exception;
                CompleteInternal();
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Queue the continuation to <see cref="SynchronizationContext.Current"/> if <see cref="_continueOnCapturedContext"/> is <see langword="true"/>,
    /// otherwise schedule it on <see cref="TaskScheduler.Default"/>
    /// </summary>
    private void CompleteInternal()
    {
        if (_continuation is null)
            return;

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

    public ReenterableAwaiter<TResult> GetAwaiter() => this;

    public void Dispose() => _semaphoreSlim.Dispose();
}
