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
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public abstract class ArtworkHistoryPersistentManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntry> : SqlitePersistentManager, IArtworkHistorySource
    where TEntry : ArtworkHistoryEntry, new()
{
    private const int MaxQueryParameterCount = 500;
    private const int StreamPageSize = 100;
    private readonly FileLogger _logger;

    protected ArtworkHistoryPersistentManager(SQLiteConnection db, FileLogger logger) : base(db)
    {
        _logger = logger;
        AccessDatabase(static connection =>
        {
            _ = connection.CreateTable<ArtworkPayloadEntry>();
            _ = connection.CreateTable<TEntry>();
        });
    }

    public int Count => AccessDatabase(static connection => connection.Table<TEntry>().Count());

    public event EventHandler? Changed;

    public IAsyncEnumerable<IArtworkInfo> StreamAsync(SimpleWorkType workType, CancellationToken token = default)
    {
        var novelSerializeKey = Novel.SerializeToken;
        return workType switch
        {
            SimpleWorkType.Novel => StreamArtworksAsync<Novel>(entry => entry.SerializeKey == novelSerializeKey, token),
            SimpleWorkType.Illustration => StreamArtworksAsync<IArtworkInfo>(entry => entry.SerializeKey == null! || entry.SerializeKey != novelSerializeKey, token),
            _ => throw new ArgumentOutOfRangeException(nameof(workType), workType, null)
        };
    }

    public IAsyncEnumerable<TEntry> StreamEntriesAsync(
        int skip = 0,
        CancellationToken token = default) =>
        StreamEntriesCoreAsync(null, skip, token);

    protected IAsyncEnumerable<TEntry> QueryEntriesAsync(
        Expression<Func<TEntry, bool>> predicate,
        int skip = 0,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return StreamEntriesCoreAsync(predicate, skip, token);
    }

    protected TEntry? FindEntry(Expression<Func<TEntry, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var snapshot = AccessDatabase(connection =>
        {
            var entry = connection.Table<TEntry>().FirstOrDefault(predicate);
            return entry is null
                ? new EntrySnapshot(null, new Dictionary<int, ArtworkPayloadEntry>())
                : new(entry, LoadPayloads(connection, [entry]));
        });
        return snapshot.Entry is null
            ? null
            : HydrateEntries([snapshot.Entry], snapshot.Payloads).FirstOrDefault();
    }

    public virtual void Insert(TEntry entry)
    {
        AccessDatabase(connection =>
        {
            connection.RunInTransaction(() => InsertCore(connection, entry));
            OnEntryInserted(entry);
        });
        OnChanged();
    }

    protected void InsertReplacing(TEntry entry, Func<TableQuery<TEntry>, TEntry?> findExisting)
    {
        ArgumentNullException.ThrowIfNull(findExisting);
        AccessDatabase(connection =>
        {
            TEntry? existing = null;
            connection.RunInTransaction(() =>
            {
                existing = findExisting(connection.Table<TEntry>());
                if (existing is not null)
                    DeleteCore(connection, existing);
                InsertCore(connection, entry);
            });
            if (existing is not null)
                OnEntriesDeleted([existing]);
            OnEntryInserted(entry);
        });
        OnChanged();
    }

    public virtual void Update(TEntry entry)
    {
        AccessDatabase(connection => _ = connection.Update(entry, typeof(TEntry)));
        OnChanged();
    }

    protected bool TryDelete(Func<TableQuery<TEntry>, TEntry?> findEntry)
    {
        ArgumentNullException.ThrowIfNull(findEntry);
        var deletedEntry = AccessDatabase(connection =>
        {
            TEntry? entry = null;
            connection.RunInTransaction(() =>
            {
                entry = findEntry(connection.Table<TEntry>());
                if (entry is not null)
                    DeleteCore(connection, entry);
            });
            if (entry is not null)
                OnEntriesDeleted([entry]);
            return entry;
        });
        if (deletedEntry is not null)
            OnChanged();
        return deletedEntry is not null;
    }

    public virtual void Clear()
    {
        var deletedCount = AccessDatabase(connection =>
        {
            var count = 0;
            connection.RunInTransaction(() =>
            {
                int? beforeHistoryEntryId = null;
                while (true)
                {
                    var query = connection.Table<TEntry>();
                    if (beforeHistoryEntryId is { } id)
                        query = query.Where(entry => entry.HistoryEntryId < id);
                    var entries = query
                        .OrderByDescending(static entry => entry.HistoryEntryId)
                        .Take(MaxQueryParameterCount)
                        .ToArray();
                    if (entries.Length is 0)
                        break;

                    DeletePayloads(
                        connection,
                        entries.Select(static entry => entry.ArtworkPayloadEntryId));
                    beforeHistoryEntryId = entries[^1].HistoryEntryId;
                }

                count = connection.DeleteAll<TEntry>();
            });
            if (count > 0)
                OnEntriesCleared();
            return count;
        });

        if (deletedCount > 0)
            OnChanged();
    }

    private async IAsyncEnumerable<TArtwork> StreamArtworksAsync<TArtwork>(
        Expression<Func<TEntry, bool>> predicate,
        [EnumeratorCancellation] CancellationToken token = default)
        where TArtwork : class, IArtworkInfo
    {
        await foreach (var entry in StreamEntriesCoreAsync(predicate, 0, token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            if (entry.Entry is TArtwork artwork)
            {
                yield return artwork;
                continue;
            }

            _logger.LogError(
                $"History entry {entry.HistoryEntryId} has an inconsistent serialize type {entry.SerializeKey}",
                null);
            DeleteBrokenEntries([entry]);
        }
    }

    private async IAsyncEnumerable<TEntry> StreamEntriesCoreAsync(
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
            foreach (var entry in page.Entries)
            {
                token.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private EntryPage LoadPage(
        int? beforeHistoryEntryId,
        Expression<Func<TEntry, bool>>? predicate,
        int initialSkip)
    {
        var rawPage = AccessDatabase<RawEntryPage>(connection =>
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
                ? new RawEntryPage([], new Dictionary<int, ArtworkPayloadEntry>(), null, false)
                : new(
                    entries,
                    LoadPayloads(connection, entries),
                    entries[^1].HistoryEntryId,
                    true);
        });
        return rawPage.HasRows
            ? new(
                HydrateEntries(rawPage.Entries, rawPage.Payloads),
                rawPage.NextHistoryEntryId,
                true)
            : new([], null, false);
    }

    private IReadOnlyList<TEntry> HydrateEntries(
        IReadOnlyList<TEntry> entries,
        IReadOnlyDictionary<int, ArtworkPayloadEntry> payloads)
    {
        if (entries.Count is 0)
            return [];

        var result = new List<TEntry>(entries.Count);
        var brokenEntries = new List<TEntry>();
        foreach (var entry in entries)
        {
            if (!payloads.TryGetValue(entry.ArtworkPayloadEntryId, out var payload))
            {
                brokenEntries.Add(entry);
                continue;
            }

            try
            {
                entry.AttachPayload(payload);
                result.Add(entry);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to deserialize history entry {entry.HistoryEntryId}", e);
                brokenEntries.Add(entry);
            }
        }

        if (brokenEntries.Count is not 0)
            DeleteBrokenEntries(brokenEntries);
        return result;
    }

    private Dictionary<int, ArtworkPayloadEntry> LoadPayloads(
        SQLiteConnection connection,
        IReadOnlyCollection<TEntry> entries)
    {
        var ids = entries
            .Select(static entry => entry.ArtworkPayloadEntryId)
            .Where(static id => id > 0)
            .Distinct()
            .ToArray();
        if (ids.Length is 0)
            return [];

        var predicate = CreateIdPredicate<ArtworkPayloadEntry>(
            static payload => payload.ArtworkPayloadEntryId,
            ids);
        return connection.Table<ArtworkPayloadEntry>()
            .Where(predicate)
            .ToDictionary(static payload => payload.ArtworkPayloadEntryId);
    }

    private void DeleteBrokenEntries(IReadOnlyCollection<TEntry> entries)
    {
        if (entries.Count is 0)
            return;

        AccessDatabase(connection =>
        {
            connection.RunInTransaction(() =>
            {
                DeleteEntries(
                    connection,
                    entries.Select(static entry => entry.HistoryEntryId));
                DeletePayloads(
                    connection,
                    entries.Select(static entry => entry.ArtworkPayloadEntryId));
            });
            OnEntriesDeleted(entries);
        });
        OnChanged();
    }

    private static void InsertCore(SQLiteConnection connection, TEntry entry)
    {
        if (entry.Payload is not { } payload)
            throw new InvalidOperationException($"{nameof(TEntry)} must have an artwork payload before it is inserted");

        if (payload.ArtworkPayloadEntryId is not 0
            && connection.Find<ArtworkPayloadEntry>(payload.ArtworkPayloadEntryId) is null)
            payload.ArtworkPayloadEntryId = 0;
        if (payload.ArtworkPayloadEntryId is 0)
            _ = connection.Insert(payload, typeof(ArtworkPayloadEntry));
        entry.ArtworkPayloadEntryId = payload.ArtworkPayloadEntryId;
        _ = connection.Insert(entry, typeof(TEntry));
    }

    private static void DeleteCore(SQLiteConnection connection, TEntry entry)
    {
        _ = connection.Delete<TEntry>(entry.HistoryEntryId);
        if (entry.ArtworkPayloadEntryId > 0)
            _ = connection.Delete<ArtworkPayloadEntry>(entry.ArtworkPayloadEntryId);
    }

    private static void DeleteEntries(
        SQLiteConnection connection,
        IEnumerable<int> sourceIds)
    {
        foreach (var ids in sourceIds.Distinct().Chunk(MaxQueryParameterCount))
            _ = connection.Table<TEntry>()
                .Delete(CreateIdPredicate<TEntry>(static entry => entry.HistoryEntryId, ids));
    }

    private static void DeletePayloads(
        SQLiteConnection connection,
        IEnumerable<int> sourceIds)
    {
        foreach (var ids in sourceIds
                     .Where(static id => id > 0)
                     .Distinct()
                     .Chunk(MaxQueryParameterCount))
            _ = connection.Table<ArtworkPayloadEntry>()
                .Delete(CreateIdPredicate<ArtworkPayloadEntry>(
                    static payload => payload.ArtworkPayloadEntryId,
                    ids));
    }

    private static Expression<Func<TModel, bool>> CreateIdPredicate<TModel>(
        Expression<Func<TModel, int>> idSelector,
        IReadOnlyList<int> ids)
    {
        if (ids.Count is 0)
            throw new ArgumentException("At least one identifier is required", nameof(ids));

        var idExpression = idSelector.Body;
        var body = Expression.Equal(idExpression, Expression.Constant(ids[0]));
        for (var i = 1; i < ids.Count; i++)
            body = Expression.OrElse(
                body,
                Expression.Equal(idExpression, Expression.Constant(ids[i])));
        return Expression.Lambda<Func<TModel, bool>>(body, idSelector.Parameters);
    }

    protected virtual void OnEntryInserted(TEntry entry)
    {
    }

    protected virtual void OnEntriesDeleted(IReadOnlyCollection<TEntry> entries)
    {
    }

    protected virtual void OnEntriesCleared()
    {
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

    private readonly record struct EntrySnapshot(
        TEntry? Entry,
        IReadOnlyDictionary<int, ArtworkPayloadEntry> Payloads);

    private readonly record struct RawEntryPage(
        IReadOnlyList<TEntry> Entries,
        IReadOnlyDictionary<int, ArtworkPayloadEntry> Payloads,
        int? NextHistoryEntryId,
        bool HasRows);

    private readonly record struct EntryPage(
        IReadOnlyList<TEntry> Entries,
        int? NextHistoryEntryId,
        bool HasRows);
}
