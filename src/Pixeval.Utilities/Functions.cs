// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public static class Functions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, T> Identity<T>()
    {
        return static t => t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ROut Let<TIn, ROut>(this TIn obj, Func<TIn, ROut> block)
    {
        return block(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Let<T>(this T obj, Action<T> block)
    {
        block(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Apply<T>(this T obj, Action<T> block)
    {
        block(obj);
        return obj;
    }

    public static async Task<Result<TResult>> WithTimeoutAsync<TResult>(Task<TResult> task, int timeoutMills)
    {
        using var cancellationToken = new CancellationTokenSource();
        if (await Task.WhenAny(task, Task.Delay(timeoutMills, cancellationToken.Token)).ConfigureAwait(false) == task)
        {
            await cancellationToken.TryCancelAsync();
            return Result<TResult>.AsSuccess(task.Result);
        }

        return Result<TResult>.AsFailure();
    }

    public static async Task<Result<TResult>> RetryAsync<TResult>(Func<Task<TResult>> body, int attempts = 3, int timeoutMs = 0)
    {
        var counter = 0;
        Exception? cause = null;
        while (counter++ < attempts)
        {
            var task = body();
            try
            {
                if (await WithTimeoutAsync(task, timeoutMs).ConfigureAwait(false) is Result<TResult>.Success result)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                cause = e;
            }
        }

        return Result<TResult>.AsFailure(cause);
    }

    public static Task<TResult> TryCatchAsync<TResult>(Func<Task<TResult>> function, Func<Exception, Task<TResult>> onException)
    {
        try
        {
            return function();
        }
        catch (Exception e)
        {
            return onException(e);
        }
    }

    public static void IgnoreException(Action action)
    {
        try
        {
            action();
        }
        catch
        {
            // ignored
        }
    }

    public static TResult Block<TResult>(Func<TResult> block)
    {
        return block();
    }

    public static async Task IgnoreExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch
        {
            // ignore
        }
    }

    public static async Task IgnoreExceptionAsync(Func<ValueTask> action)
    {
        try
        {
            await action();
        }
        catch
        {
            // ignore
        }
    }
}
