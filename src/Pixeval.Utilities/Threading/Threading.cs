#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2025 Pixeval.Utilities/Threading.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities.Threading;

public static class Threading
{
    public static Task WaitOneAsync(this WaitHandle waitHandle)
    {
        if (waitHandle == null)
        {
            throw new ArgumentNullException(nameof(waitHandle));
        }

        var tcs = new TaskCompletionSource<bool>();
        var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
            (_, _) => tcs.TrySetResult(true), null, -1, true);
        var t = tcs.Task;
        _ = t.ContinueWith(_ => rwh.Unregister(null));
        return t;
    }

    public static Task WaitAnyAsync(WaitHandle[] waitHandles)
    {
        return Task.WhenAny(waitHandles.Select(wh => wh.WaitOneAsync()));
    }
}
