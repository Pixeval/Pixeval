#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2021 Pixeval.Utilities/Functions.cs
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

namespace Pixeval.Utilities;

public static class Functions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, T> Identity<T>()
    {
        return static t => t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ROut? Let<TIn, ROut>(this TIn obj, Func<TIn, ROut> block)
    {
        return obj is not null ? block(obj) : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Let<T>(this T obj, Action<T> block)
    {
        if (obj is not null)
        {
            block(obj);
        }
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
            cancellationToken.Cancel();
            return Result<TResult>.OfSuccess(task.Result);
        }

        return Result<TResult>.OfFailure();
    }

    public static async Task<Result<TResult>> RetryAsync<TResult>(Func<Task<TResult>> body, int attempts = 3, int timeoutMills = 0)
    {
        var counter = 0;
        Exception? cause = null;
        while (counter++ < attempts)
        {
            var task = body();
            try
            {
                if (await WithTimeoutAsync(task, timeoutMills).ConfigureAwait(false) is Result<TResult>.Success result)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                cause = e;
            }
        }

        return Result<TResult>.OfFailure(cause);
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
}