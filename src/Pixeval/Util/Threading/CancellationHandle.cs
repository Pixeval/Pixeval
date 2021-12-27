#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CancellationHandle.cs
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
using System.Threading;

namespace Pixeval.Util.Threading;

/// <summary>
///     A re-entrant cancellation helper that prevents the default behaviors of <see cref="CancellationToken" />, which is,
///     throws an <see cref="OperationCanceledException" />
/// </summary>
public class CancellationHandle
{
    private int _isCancelled;

    private Action? _onCancellation;

    private Action? _onPause;

    private Action? _onResume;

    private int _paused;

    public bool IsCancelled => _isCancelled == 1;

    public bool IsPaused => _paused == 1;

    public void Cancel()
    {
        if (!IsCancelled)
        {
            _onCancellation?.Invoke();
            Interlocked.Increment(ref _isCancelled);
        }
    }

    public void Reset()
    {
        if (IsPaused)
        {
            Interlocked.Decrement(ref _paused);
        }

        if (IsCancelled)
        {
            Interlocked.Decrement(ref _isCancelled);
        }
    }

    public void Pause()
    {
        if (!IsPaused)
        {
            _onPause?.Invoke();
            Interlocked.Increment(ref _paused);
        }
    }

    public void Resume()
    {
        if (IsPaused)
        {
            _onResume?.Invoke();
            Interlocked.Decrement(ref _paused);
        }
    }

    public void Register(Action action)
    {
        _onCancellation += action;
    }

    public void RegisterPaused(Action action)
    {
        _onPause += action;
    }

    public void RegisterResumed(Action action)
    {
        _onResume += action;
    }
}