// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public abstract class PersistentManagerBase<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntry, TModel>
    : SqlitePersistentManager, IPersistentManager<TEntry, TModel>
    where TEntry : HistoryEntry, new()
{
    private const int StreamPageSize = 100;

    protected PersistentManagerBase(SQLiteConnection db) : base(db) =>
        AccessDatabase(static connection => _ = connection.CreateTable<TEntry>());

    /// <inheritdoc />
    public virtual int Count => AccessDatabase(static connection => connection.Table<TEntry>().Count());

    /// <inheritdoc />
    public virtual void Insert(TEntry entry) =>
        AccessDatabase(connection => _ = connection.Insert(entry, typeof(TEntry)));

    public IAsyncEnumerable<TModel> StreamEntriesAsync(int skip = 0, CancellationToken token = default) =>
        StreamEntriesCoreAsync(null, skip, token);

    protected IAsyncEnumerable<TModel> QueryEntriesAsync(
        Expression<Func<TEntry, bool>> predicate,
        int skip = 0,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return StreamEntriesCoreAsync(predicate, skip, token);
    }

    /// <inheritdoc />
    public virtual void AddOrUpdate(TEntry entry) =>
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));

    /// <inheritdoc />
    public virtual TEntry Upsert(TEntry entry)
    {
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));
        return entry;
    }

    /// <inheritdoc />
    public virtual void Update(TEntry entry) =>
        AccessDatabase(connection => _ = connection.Update(entry, typeof(TEntry)));

    /// <inheritdoc />
    public bool TryDelete(TEntry item) =>
        AccessDatabase(connection => connection.Delete<TEntry>(item.HistoryEntryId) is not 0);

    /// <inheritdoc />
    public virtual TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return AccessDatabase<TEntry?>(connection =>
        {
            if (connection.Table<TEntry>().FirstOrDefault(predicate) is not { } entry)
                return null;

            _ = connection.Delete<TEntry>(entry.HistoryEntryId);
            return entry;
        });
    }

    /// <inheritdoc />
    public virtual int Delete(Expression<Func<TEntry, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return AccessDatabase(connection => connection.Table<TEntry>().Delete(predicate));
    }

    /// <inheritdoc />
    public virtual void Clear() => AccessDatabase(static connection => _ = connection.DeleteAll<TEntry>());

    protected abstract TModel ToModel(TEntry entry);

    private static void AddOrUpdateCore(SQLiteConnection connection, TEntry entry)
    {
        if (entry.HistoryEntryId is 0)
            _ = connection.Insert(entry, typeof(TEntry));
        else
            _ = connection.InsertOrReplace(entry, typeof(TEntry));
    }

    private async IAsyncEnumerable<TModel> StreamEntriesCoreAsync(
        Expression<Func<TEntry, bool>>? predicate,
        int skip,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        int? beforeHistoryEntryId = null;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var page = await Task.Run(
                    () => LoadPage(beforeHistoryEntryId, predicate, skip),
                    token)
                .ConfigureAwait(false);
            if (!page.HasRows)
                yield break;

            beforeHistoryEntryId = page.NextHistoryEntryId;
            foreach (var model in page.Models)
            {
                token.ThrowIfCancellationRequested();
                yield return model;
            }
        }
    }

    private EntryPage LoadPage(
        int? beforeHistoryEntryId,
        Expression<Func<TEntry, bool>>? predicate,
        int initialSkip)
    {
        return AccessDatabase<EntryPage>(connection =>
        {
            var query = connection.Table<TEntry>();
            if (predicate is not null)
                query = query.Where(predicate);
            if (beforeHistoryEntryId is { } id)
                query = query.Where(entry => entry.HistoryEntryId < id);

            query = query.OrderByDescending(static entry => entry.HistoryEntryId);
            if (beforeHistoryEntryId is null && initialSkip > 0)
                query = query.Skip(initialSkip);
            var entries = query.Take(StreamPageSize).ToArray();
            return entries.Length is 0
                ? new([], null, false)
                : new([.. entries.Select(ToModel)], entries[^1].HistoryEntryId, true);
        });
    }

    private readonly record struct EntryPage(
        IReadOnlyList<TModel> Models,
        int? NextHistoryEntryId,
        bool HasRows);
}
