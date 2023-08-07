#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AsyncThrottle.cs
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
using System.Threading.Tasks;

namespace Pixeval.Util.Threading;

public sealed class AsyncFunc(Func<Task> action)
{
    private bool _running = false;

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
    private bool _running = false;

    public async Task RunAsync(T param)
    {
        if (_running)
            return;
        _running = true;
        await action(param);
        _running = false;
    }
}
