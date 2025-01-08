// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;

namespace Pixeval.Utilities;

public class Inspector
{
    [ThreadStatic]
    public static readonly Inspector Instance;

    private readonly Stopwatch _stopWatch;

    static Inspector()
    {
        Instance = new Inspector();
    }

    private Inspector()
    {
        _stopWatch = new Stopwatch();
    }

    public void TimerStart(string message)
    {
        Debug.WriteLine(message);
        if (_stopWatch.IsRunning)
        {
            throw new InvalidOperationException("The timer is already occupied by another task, please release it first.");
        }
        _stopWatch.Start();
    }

    public void TimerEnd(string message)
    {
        if (!_stopWatch.IsRunning)
        {
            throw new InvalidOperationException("The timer is yet to start.");
        }
        Debug.WriteLine($"[{_stopWatch.Elapsed}] {message}");
        _stopWatch.Reset();
    }
}
