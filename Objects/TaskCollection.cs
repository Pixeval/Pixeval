// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Objects
{
    public class TaskCollection<T, TResult>
    {
        private readonly IEnumerable<Task<TResult>> tasks;

        public TaskCollection(IEnumerable<T> enumerable, Func<IEnumerable<T>, IEnumerable<Task<TResult>>> taskSelector)
        {
            tasks = taskSelector(enumerable);
        }

        public async Task<TResult[]> WhenAll()
        {
            return await Task.WhenAll(tasks);
        }

        public async Task<Task<TResult>> WhenAny()
        {
            return await Task.WhenAny(tasks);
        }

        public class Builder
        {
            private IEnumerable<T> taskList;

            private Func<IEnumerable<T>, IEnumerable<Task<TResult>>> taskSelector;

            public Builder On(IEnumerable<T> enumerable)
            {
                taskList = enumerable;
                return this;
            }

            public Builder Selector(Func<IEnumerable<T>, IEnumerable<Task<TResult>>> selector)
            {
                taskSelector = selector;
                return this;
            }

            public TaskCollection<T, TResult> Build()
            {
                return new TaskCollection<T, TResult>(taskList, taskSelector);
            }
        }
    }
}