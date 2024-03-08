#region Copyright (c) $SOLUTION$/$PROJECT$
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/Debug.cs
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
