// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace Pixeval.Database.Managers;

/// <summary>
/// Manage persistent data stored in SQLite.
/// You may call CreateTable in the constructor.
/// </summary>
/// <remarks>
/// <example>
/// This example shows how to get a registered manager.
/// <code>
/// var manager = <see cref="App.AppViewModel"/>.AppServiceProvider.GetRequiredService&lt;<see cref="IPersistentManager{T, T}"/>&gt;();
/// </code>
/// </example>
/// <see cref="AppViewModel.CreateServiceProvider">Register the manager in AppViewModel</see>
/// </remarks>
/// <typeparam name="TEntry">Entry to be serialized in database</typeparam>
/// <typeparam name="TModel">Data model in the program</typeparam>
public interface IPersistentManager<TEntry, out TModel> where TEntry : IHistoryEntry
{
    ILiteCollection<TEntry> Collection { get; init; }

    int MaximumRecords { get; set; }

    int Count { get; }

    void Insert(TEntry t);

    IEnumerable<TModel> Query(Expression<Func<TEntry, bool>> predicate);

    void Update(TEntry entry);

    IEnumerable<TModel> Take(int count);

    IEnumerable<TModel> TakeLast(int count);

    IEnumerable<TModel> Select(Expression<Func<TEntry, bool>> predicate);

    TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate);

    int Delete(Expression<Func<TEntry, bool>> predicate);

    IEnumerable<TModel> Enumerate();

    IEnumerable<TModel> Reverse();

    void Purge(int limit);

    void Clear();
}
