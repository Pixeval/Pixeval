// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Pixeval.Controls.Windowing;

namespace Pixeval.Util.Threading;

public static class ThreadingHelper
{
    public static Task<TR> ContinueWithFlatten<T, TR>(this Task<T> task, Func<T, Task<TR>> func)
    {
        return task.ContinueWith(t => func(t.Result)).Unwrap();
    }

    public static void CompareExchange(ref int location1, int value, int comparand)
    {
        while (Interlocked.CompareExchange(ref location1, value, comparand) != comparand)
        {
        }
    }

    // fork a task from current context.
    public static Task Fork(Func<Task> action)
    {
        return action();
    }

    public static Task<TResult> Fork<TResult>(Func<Task<TResult>> action)
    {
        return action();
    }

    public static void DispatchTask(DispatcherQueueHandler action)
    {
        _ = WindowFactory.RootWindow.DispatcherQueue.TryEnqueue(action);
    }

    public static Task<T> DispatchSyncTaskAsync<T>(Func<T> func)
    {
        return WindowFactory.RootWindow.DispatcherQueue.EnqueueSyncTaskAsync(func);
    }

    public static Task DispatchTaskAsync(Func<Task> action)
    {
        return WindowFactory.RootWindow.DispatcherQueue.EnqueueAsync(action);
    }

    public static Task<T> DispatchTaskAsync<T>(Func<Task<T>> action)
    {
        return WindowFactory.RootWindow.DispatcherQueue.EnqueueAsync(action);
    }

    public static Task<T> EnqueueSyncTaskAsync<T>(this DispatcherQueue dispatcher, Func<T> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        _ = dispatcher.TryEnqueue(priority, () =>
        {
            try
            {
                _ = taskCompletionSource.TrySetResult(function());
            }
            catch (Exception e)
            {
                _ = taskCompletionSource.TrySetException(e);
            }
        });
        return taskCompletionSource.Task;
    }

    public static Task SpinWaitAsync(Func<bool> condition)
    {
        var tcs = new TaskCompletionSource();
        _ = Task.Run(async () =>
        {
            var spinWait = new SpinWait();
            while (await DispatchSyncTaskAsync(condition))
            {
                spinWait.SpinOnce(20);
            }

            tcs.SetResult();
        });
        return tcs.Task;
    }
}
