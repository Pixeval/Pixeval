#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Functions.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
{
    [PublicAPI]
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
    }
}