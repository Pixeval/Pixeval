#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Objects.Generic
{
    public static class Functions
    {
        public static T Apply<T>(this T receiver, Action<T> action)
        {
            action(receiver);
            return receiver;
        }

        public static Task AwaitAsync<T>(this T obj, Func<T, Task<bool>> on, int interval = 0, TimeSpan timeout = default)
        {
            var timer = DateTime.Now;
            return Task.Run(async () =>
            {
                while (!await on(obj))
                {
                    if (timeout != default && DateTime.Now - timer >= timeout)
                    {
                        break;
                    }
                    if (interval != 0)
                    {
                        await Task.Delay(interval);
                    }
                }
            });
        }
    }
}