// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;

namespace Pixeval.Util.Threading;

public sealed class AsyncFunc(Func<Task> action)
{
    private bool _running;

    public async Task RunAsync()
    {
        if (_running)
            return;
        _running = true;
        await action();
        _running = false;
    }
}

public sealed class AsyncFunc<T>(Func<T, Task> action)
{
    private bool _running;

    public async Task RunAsync(T param)
    {
        if (_running)
            return;
        _running = true;
        await action(param);
        _running = false;
    }
}
