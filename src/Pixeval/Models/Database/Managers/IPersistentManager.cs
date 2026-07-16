// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Pixeval.AppManagement;

namespace Pixeval.Models.Database.Managers;

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
public interface IPersistentManager<TEntry, out TModel> where TEntry : HistoryEntry
{
    int Count { get; }

    IAsyncEnumerable<TModel> StreamEntriesAsync(int skip = 0, CancellationToken token = default);

    void Insert(TEntry entry);

    void AddOrUpdate(TEntry entry);

    TEntry Upsert(TEntry entry);

    void Update(TEntry entry);

    bool TryDelete(TEntry entry);

    TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate);

    int Delete(Expression<Func<TEntry, bool>> predicate);

    void Clear();
}
