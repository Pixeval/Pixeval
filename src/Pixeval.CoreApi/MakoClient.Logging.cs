using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.Utilities;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    private async Task<T[]> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<T[]>> task)
    {
        try
        {
            EnsureNotCancelled();

            return await task(Resolve<IAppApiEndPoint>());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            return [];
        }
    }

    private async Task<IEnumerable<T>> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<IEnumerable<T>>> task)
    {
        try
        {
            EnsureNotCancelled();

            return await task(Resolve<IAppApiEndPoint>());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            return [];
        }
    }

    private async Task RunWithLoggerAsync(Func<IAppApiEndPoint, Task> task)
    {
        try
        {
            EnsureNotCancelled();

            await task(Resolve<IAppApiEndPoint>());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
        }
    }

    private async Task<T> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<T>> task) where T : IFactory<T>
    {
        try
        {
            EnsureNotCancelled();

            return await task(Resolve<IAppApiEndPoint>());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            return T.CreateDefault();
        }
    }

    private async Task<T> RunWithLoggerAsync<T>(Func<Task<T>> task) where T : IFactory<T>
    {
        try
        {
            EnsureNotCancelled();

            return await task();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            return T.CreateDefault();
        }
    }

    private async Task<T> RunWithLoggerAsync<T>(Func<Task<Result<T>>> task, Func<T> createDefault)
    {
        try
        {
            EnsureNotCancelled();

            var result = await task();
            switch (result)
            {
                case Result<T>.Success { Value: var value }:
                    return value;
                case Result<T>.Failure { Cause: var e }:
                    Logger.LogError(e, "");
                    return createDefault();
                default:
                    return ThrowUtils.ArgumentOutOfRange<Result<T>, T>(result);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            return createDefault();
        }
    }

    private Task<T> RunWithLoggerAsync<T>(Func<Task<Result<T>>> task) where T : IFactory<T>
    {
        return RunWithLoggerAsync(task, T.CreateDefault);
    }

    //private async Task<T> RunWithLoggerAsync<T>(Func<Task<Result<T>>> task, T defaultValue)
    //{
    //    try
    //    {
    //        EnsureNotCancelled();

    //        var result = await task();
    //        switch (result)
    //        {
    //            case Result<T>.Success { Value: var value }:
    //                return value;
    //            case Result<T>.Failure { Cause: var e }:
    //                Logger.LogError(e, "");
    //                return defaultValue;
    //            default:
    //                return ThrowUtils.ArgumentOutOfRange<Result<T>, T>(result);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Logger.LogError(e, "");
    //        return defaultValue;
    //    }
    //}
}
