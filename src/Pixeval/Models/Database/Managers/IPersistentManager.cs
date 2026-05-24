// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using SQLite;

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
public interface IPersistentManager<TEntry, out TModel> : IWriteOnlyPersistentManager<TEntry>, IEnumerable<TModel> where TEntry : HistoryEntry
{
    IReadOnlyList<TModel> Query(Expression<Func<TEntry, bool>> predicate, int skip = 0, int limit = int.MaxValue);

    IReadOnlyList<TModel> Take(int count);

    IReadOnlyList<TModel> TakeLast(int count);

    IReadOnlyList<TModel> Select(Expression<Func<TEntry, bool>> predicate);

    IReadOnlyList<TModel> ToArray();

    IReadOnlyList<TModel> Reverse();
}

public interface IWriteOnlyPersistentManager<TEntry> where TEntry : HistoryEntry
{
    TableQuery<TEntry> Queryable { get; }

    int Count { get; }

    void Insert(TEntry t);

    void AddOrUpdate(TEntry entry);

    TEntry Upsert(TEntry entry);

    void Update(TEntry entry);

    bool TryDelete(TEntry item);

    TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate);

    int Delete(Expression<Func<TEntry, bool>> predicate);

    /// <summary>
    /// 清除多于<paramref name="limit"/>的记录
    /// </summary>
    /// <param name="limit"></param>
    void Purge(int limit);

    /// <summary>
    /// 清除所有记录
    /// </summary>
    void Clear();
}
