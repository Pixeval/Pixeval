#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2024 Pixeval.Utilities/Debounce.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public interface IDebouncedTask<T, TResult> where T : struct, IEquatable<T>
{
    T Id { get; }

    T? Dependency { get; }

    Task<TResult> ExecuteAsync();

    bool IsFinalizer { get; }
}

/// <summary>
/// This class is used to debounce tasks that are dependent on each other, a common <c>debounce()</c> function often
/// debouncing per task basis, however, sometimes tasks can be grouped, this class debounces specifically the tasks
/// that are dependent on each other.
///
/// To use this class, a tag of type <see cref="T"/> is required, the tag identifies each task and its dependency,
/// the class debounces tasks in the following way:
///
/// 1. If a task has a dependency, it will be executed only if the dependency has been executed, otherwise it will
///    be debounced (disregarded)
/// 2. If a task has already been executed, it will be debounced (disregarded)
/// 3. If a task is a finalizer, then it finalizes the task group, the whole chain of dependency will be removed from
///    the executed tasks list, allows this group of task to be executed once again.
/// 4. Pre-debouncing:
///     1. If a task is the same as the last task, it will be debounced (disregarded)
///     2. If a task group is the same as the last task group (executing), it will be debounced (disregarded)
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class Debounce<T, TResult> : IDisposable where T : struct, IEquatable<T>
{
    private record DebounceTaskWrapper(bool Disregarded, IDebouncedTask<T, TResult> Task, TaskCompletionSource<TResult> Completion)
    {
        public bool Disregarded { get; set; } = Disregarded;
    }

    private bool _started;
    private readonly LinkedList<DebounceTaskWrapper> _executedTasks = [];
    private readonly Channel<DebounceTaskWrapper> _taskQueue = Channel.CreateUnbounded<DebounceTaskWrapper>();
    private readonly List<DebounceTaskWrapper> _auxQueue = [];

    public async Task<TResult> ExecuteAsync(IDebouncedTask<T, TResult> task)
    {
        if (_taskQueue.Reader.Completion.IsCompleted)
        {
            throw new InvalidOperationException("The debounce queue has been disposed");
        }

        if (!_started)
            _ = StartLoopAsync();

        var wrapper = new DebounceTaskWrapper(false, task, new TaskCompletionSource<TResult>());

        if (_auxQueue.LastOrDefault()?.Task.Id.Equals(task.Id) ?? false)
        {
            return await Task.FromCanceled<TResult>(new CancellationToken(true));
        }

        _auxQueue.Add(wrapper);

        if (task.IsFinalizer)
        {
            // try debouncing the whole group
            var dependencyChain = FindDependencyChainFrom(new LinkedList<DebounceTaskWrapper>(_auxQueue), wrapper);
            var truncated = new LinkedList<DebounceTaskWrapper>(_auxQueue[..^dependencyChain.Count]);
            var oldDependencyChain = FindDependencyChainOf(truncated, task.Id);
            if (dependencyChain.SequenceEquals(oldDependencyChain, ts => ts.Task.Id))
            {
                foreach (var debouncedTaskWrapper in dependencyChain)
                {
                    debouncedTaskWrapper.Disregarded = true;
                }
            }
        }

        await _taskQueue.Writer.WriteAsync(wrapper);
        return await wrapper.Completion.Task;
    }

    private async Task StartLoopAsync()
    {
        if (_started)
            return;
        _started = true;
        while (await _taskQueue.Reader.WaitToReadAsync())
        {
            var wrapper = await _taskQueue.Reader.ReadAsync();
            var (disregarded, task, completion) = wrapper;

            if (disregarded)
            {
                completion.SetCanceled();
            }

            // TODO comment this temporarily for some tasks can be both grouped or singleton
            // if (task.Dependency is { } dep && _executedTasks.All(t => !t.Task.Id.Equals(dep)))
            // {
            //     completion.SetCanceled();
            //     continue;
            // }

            if (_executedTasks.Any(t => t.Task.Id.Equals(task.Id)))
            {
                completion.SetCanceled();
                continue;
            }

            // it's important to add task to list before executing it
            _executedTasks.AddLast(wrapper);
            var result = await task.ExecuteAsync();
            _auxQueue.RemoveAt(0);

            if (task.IsFinalizer)
            {
                var dependencyChain = FindDependencyChainFrom(_executedTasks, wrapper);
                foreach (var t in dependencyChain)
                {
                    _executedTasks.Remove(t);
                }
            }

            completion.SetResult(result);
        }
    }

    private static List<DebounceTaskWrapper> FindDependencyChainOf(LinkedList<DebounceTaskWrapper> executedTasks, T id)
    {
        var current = executedTasks.LastOrDefault(t => t.Task.Id.Equals(id));

        return current is null ? [] : FindDependencyChainFrom(executedTasks, current);
    }

    private static List<DebounceTaskWrapper> FindDependencyChainFrom(LinkedList<DebounceTaskWrapper> executedTasks, DebounceTaskWrapper current)
    {
        List<DebounceTaskWrapper> list = [current];

        for (var node = executedTasks.Last; node is { Value: { Task: var val } wrapper }; node = node.Previous)
        {
            if (val.Id.Equals(current.Task.Dependency))
            {
                list.Add(wrapper);
                current = wrapper;
            }
        }

        return list;
    }

    public void Dispose()
    {
        _taskQueue.Writer.Complete();
        GC.SuppressFinalize(this);
    }
}
