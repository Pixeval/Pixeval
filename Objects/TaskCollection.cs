using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pzxlane.Objects
{
    public class TaskCollection<T, TResult>
    {
        private readonly IEnumerable<Task<TResult>> tasks;

        public TaskCollection(IEnumerable<T> enumerable, Func<IEnumerable<T>, IEnumerable<Task<TResult>>> taskSelector)
        {
            tasks = taskSelector(enumerable);
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

        public async Task<TResult[]> WhenAll()
        {
            return await Task.WhenAll(tasks);
        }

        public async Task<Task<TResult>> WhenAny()
        {
            return await Task.WhenAny(tasks);
        }
    }
}