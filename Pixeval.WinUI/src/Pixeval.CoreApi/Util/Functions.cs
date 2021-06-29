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
        public static Func<T, T> Identity<T>() => static t => t;

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
        

        public static async Task<Result<TResult>> WithTimeout<TResult>(Task<TResult> task, int timeoutMills)
        {
            using var cancellationToken = new CancellationTokenSource();
            if (await Task.WhenAny(task, Task.Delay(timeoutMills, cancellationToken.Token)) == task)
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
                using var cancellationToken = new CancellationTokenSource();
                try
                {
                    if (await WithTimeout(task, timeoutMills) is Result<TResult>.Success result)
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