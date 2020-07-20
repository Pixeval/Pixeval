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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixeval.Objects.Generic
{
    public class Tasks<T, TR>
    {
        private Func<T, Task<TR>> _mappingFunc;
        private IEnumerable<T> _taskQueue;

        private Tasks()
        {
        }

        public static Tasks<T, TR> Of(IEnumerable<T> tasks)
        {
            return new Tasks<T, TR> { _taskQueue = tasks.NonNull() };
        }

        public Tasks<T, TR> Mapping(Func<T, Task<TR>> map)
        {
            _mappingFunc = map;
            return this;
        }

        public IEnumerable<Task<TR>> Construct()
        {
            return _taskQueue.Select(taskObj => _mappingFunc(taskObj));
        }
    }

    public static class TaskHelper
    {
        public static Task<TR[]> WhenAll<TR>(this IEnumerable<Task<TR>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<Task<TR>> WhenAny<TR>(this IEnumerable<Task<TR>> tasks)
        {
            return Task.WhenAny(tasks);
        }
    }
}
