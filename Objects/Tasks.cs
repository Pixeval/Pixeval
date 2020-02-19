// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixeval.Objects
{
    public class Tasks<T, R>
    {
        private Func<T, Task<R>> mappingFunc;
        private IEnumerable<T> taskQueue;

        private Tasks() { }

        public static Tasks<T, R> Of(IEnumerable<T> tasks)
        {
            return new Tasks<T, R> {taskQueue = tasks.NonNull()};
        }

        public Tasks<T, R> Mapping(Func<T, Task<R>> map)
        {
            mappingFunc = map;
            return this;
        }

        public IEnumerable<Task<R>> Construct()
        {
            return taskQueue.Select(taskObj => mappingFunc(taskObj));
        }
    }

    public static class TaskHelper
    {
        public static Task<R[]> WhenAll<R>(this IEnumerable<Task<R>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<Task<R>> WhenAny<R>(this IEnumerable<Task<R>> tasks)
        {
            return Task.WhenAny(tasks);
        }
    }
}