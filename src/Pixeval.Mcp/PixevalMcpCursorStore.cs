// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Engine;
using Mako.Model;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

internal sealed class PixevalMcpCursorStore : IAsyncDisposable
{
    private const string CursorPrefix = "pixeval:";

    private const int MaxActiveCursors = 64;

    private static readonly TimeSpan _CursorTtl = TimeSpan.FromMinutes(10);

    private readonly Lock _lock = new();

    private readonly Dictionary<Guid, CursorEntry> _entries = [];

    public async Task<PixevalWorkListDto> CreateWorkCursorAsync(
        IFetchEngine<IWorkEntry> engine,
        IPixevalMcpRuntime runtime,
        int count,
        string? workFilter,
        PixevalWorkFilterAnalysisDto? filter,
        CancellationToken cancellationToken)
    {
        var entry = new WorkCursorEntry(engine, runtime.CurrentUser?.Id, workFilter, filter);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                page.Count,
                page.Works ?? [],
                page.Filter,
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalUserListDto> CreateUserCursorAsync(
        IFetchEngine<User> engine,
        IPixevalMcpRuntime runtime,
        int count,
        CancellationToken cancellationToken)
    {
        var entry = new UserCursorEntry(engine, runtime.CurrentUser?.Id);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                page.Count,
                page.Users ?? [],
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalSeriesListDto> CreateSeriesCursorAsync(
        IFetchEngine<Series> engine,
        IPixevalMcpRuntime runtime,
        int count,
        CancellationToken cancellationToken)
    {
        var entry = new SeriesCursorEntry(engine, runtime.CurrentUser?.Id);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                page.Count,
                page.Series ?? [],
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalSpotlightListDto> CreateSpotlightCursorAsync(
        IFetchEngine<Spotlight> engine,
        IPixevalMcpRuntime runtime,
        int count,
        CancellationToken cancellationToken)
    {
        var entry = new SpotlightCursorEntry(engine, runtime.CurrentUser?.Id);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                page.Count,
                page.Spotlights ?? [],
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalCommentListDto> CreateCommentCursorAsync(
        IFetchEngine<Comment> engine,
        IPixevalMcpRuntime runtime,
        int count,
        CancellationToken cancellationToken)
    {
        var entry = new CommentCursorEntry(engine, runtime.CurrentUser?.Id);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                page.Count,
                page.Comments ?? [],
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalBookmarkTagListDto> CreateBookmarkTagCursorAsync(
        IFetchEngine<BookmarkTag> engine,
        IPixevalMcpRuntime runtime,
        long userId,
        string workType,
        string privacy,
        int count,
        CancellationToken cancellationToken)
    {
        var entry = new BookmarkTagCursorEntry(engine, runtime.CurrentUser?.Id, userId, workType, privacy);
        try
        {
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (page.HasMore)
                await AddAsync(entry).ConfigureAwait(false);
            else
                await entry.DisposeAsync().ConfigureAwait(false);

            return new(
                userId,
                workType,
                privacy,
                page.Count,
                page.BookmarkTags ?? [],
                page.HasMore,
                page.NextCursor);
        }
        catch
        {
            await entry.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<PixevalCursorPageDto> MoreAsync(
        IPixevalMcpRuntime runtime,
        string cursor,
        int count,
        CancellationToken cancellationToken)
    {
        if (!TryParseCursor(cursor, out var kind, out var id))
            throw new PixevalMcpException("The MCP cursor is invalid.");

        await CleanupAsync().ConfigureAwait(false);

        CursorEntry entry;
        lock (_lock)
        {
            if (!_entries.TryGetValue(id, out entry!) || entry.Kind != kind)
                throw new PixevalMcpException("The MCP cursor is expired, completed, or belongs to another result type.");

            if (!entry.TryEnter())
                throw new PixevalMcpException("The MCP cursor is already being read. Retry after the current request finishes.");
        }

        var shouldDispose = false;
        if (entry.UserId != runtime.CurrentUser?.Id)
        {
            shouldDispose = Remove(entry);
            try
            {
                throw new PixevalMcpException("The MCP cursor is no longer valid because the Pixeval account changed.");
            }
            finally
            {
                entry.Exit();
                if (shouldDispose)
                    await entry.DisposeAsync().ConfigureAwait(false);
            }
        }

        try
        {
            entry.LastAccessUtc = DateTimeOffset.UtcNow;
            var page = await entry.ReadPageAsync(runtime, count, cancellationToken).ConfigureAwait(false);
            if (!page.HasMore)
                shouldDispose = Remove(entry);

            return page;
        }
        finally
        {
            entry.Exit();
            if (shouldDispose)
                await entry.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        CursorEntry[] entries;
        lock (_lock)
        {
            entries = [.. _entries.Values];
            _entries.Clear();
        }

        foreach (var entry in entries)
            await entry.DisposeAsync().ConfigureAwait(false);
    }

    private async Task AddAsync(CursorEntry entry)
    {
        await CleanupAsync().ConfigureAwait(false);

        CursorEntry[] evicted;
        lock (_lock)
        {
            _entries[entry.Id] = entry;
            evicted = _entries.Count <= MaxActiveCursors
                ? []
                :
                [
                    .. _entries.Values
                        .Where(static item => item.CanDispose)
                        .OrderBy(static item => item.LastAccessUtc)
                        .Take(_entries.Count - MaxActiveCursors)
                ];

            foreach (var item in evicted)
                _entries.Remove(item.Id);
        }

        foreach (var item in evicted)
            await item.DisposeAsync().ConfigureAwait(false);
    }

    private async Task RemoveAsync(CursorEntry entry)
    {
        if (Remove(entry))
            await entry.DisposeAsync().ConfigureAwait(false);
    }

    private bool Remove(CursorEntry entry)
    {
        lock (_lock)
            return _entries.Remove(entry.Id);
    }

    private async Task CleanupAsync()
    {
        CursorEntry[] expired;
        lock (_lock)
        {
            expired = [.. _entries.Values.Where(static entry => entry.IsExpired && entry.CanDispose)];
            foreach (var entry in expired)
                _entries.Remove(entry.Id);
        }

        foreach (var entry in expired)
            await entry.DisposeAsync().ConfigureAwait(false);
    }

    private static bool TryParseCursor(string cursor, out string kind, out Guid id)
    {
        kind = "";
        id = Guid.Empty;

        if (!cursor.StartsWith(CursorPrefix, StringComparison.Ordinal))
            return false;

        var rest = cursor[CursorPrefix.Length..];
        var separator = rest.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0 || separator == rest.Length - 1)
            return false;

        kind = rest[..separator];
        return kind is PixevalMcpCursorKind.Work
                   or PixevalMcpCursorKind.User
                   or PixevalMcpCursorKind.Series
                   or PixevalMcpCursorKind.Spotlight
                   or PixevalMcpCursorKind.Comment
                   or PixevalMcpCursorKind.BookmarkTag
               && Guid.TryParseExact(rest[(separator + 1)..], "N", out id);
    }

    private abstract class CursorEntry(
        string kind,
        EngineHandle engineHandle,
        long? userId) : IAsyncDisposable
    {
        public Guid Id => engineHandle.Id;

        public string Kind { get; } = kind;

        public long? UserId { get; } = userId;

        public DateTimeOffset LastAccessUtc { get; set; } = DateTimeOffset.UtcNow;

        private SemaphoreSlim Semaphore { get; } = new(1, 1);

        protected string Token => $"{CursorPrefix}{Kind}:{Id:N}";

        public bool IsExpired => DateTimeOffset.UtcNow - LastAccessUtc > _CursorTtl;

        public bool CanDispose => Semaphore.CurrentCount > 0;

        public bool TryEnter() => Semaphore.Wait(0);

        public void Exit() => Semaphore.Release();

        public abstract Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken);

        public abstract ValueTask DisposeAsync();

        protected static async Task<(IReadOnlyList<T> Items, bool HasMore)> ReadItemsAsync<T>(
            IAsyncEnumerator<T> enumerator,
            int count,
            Ref<T?> buffer,
            CancellationToken cancellationToken)
        {
            var normalizedCount = PixevalMcpHelpers.ClampCount(count);
            var items = new List<T>(normalizedCount);
            if (buffer.Value is { } buffered)
            {
                items.Add(buffered);
                buffer.Value = default;
            }

            while (items.Count < normalizedCount)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    return (items, false);

                items.Add(enumerator.Current);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                return (items, false);

            buffer.Value = enumerator.Current;
            return (items, true);
        }

        protected async ValueTask DisposeCoreAsync<T>(IAsyncEnumerator<T> enumerator)
        {
            engineHandle.Cancel();
            await enumerator.DisposeAsync().ConfigureAwait(false);
            Semaphore.Dispose();
        }
    }

    private sealed class WorkCursorEntry(
        IFetchEngine<IWorkEntry> engine,
        long? userId,
        string? workFilter,
        PixevalWorkFilterAnalysisDto? filter)
        : CursorEntry(PixevalMcpCursorKind.Work, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<IWorkEntry> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<IWorkEntry?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            var works = items.Select(ToWorkBase).ToArray();
            runtime.CacheWorks(works);
            var filtered = string.IsNullOrWhiteSpace(workFilter)
                ? works
                : runtime.FilterWorks(works, workFilter).ToArray();
            var dtos = filtered.Select(PixevalWorkDto.FromWork).ToArray();
            return new(
                Kind,
                dtos.Length,
                hasMore,
                hasMore ? Token : null,
                Works: dtos,
                Filter: filter);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);

        private static WorkBase ToWorkBase(IWorkEntry work) =>
            work as WorkBase
            ?? throw new PixevalMcpException("Pixiv returned a work shape that Pixeval MCP cannot serialize yet.");
    }

    private sealed class UserCursorEntry(IFetchEngine<User> engine, long? userId)
        : CursorEntry(PixevalMcpCursorKind.User, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<User> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<User?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            runtime.CacheUsers(items);
            var users = items.Select(PixevalUserDto.FromUser).ToArray();
            return new(
                Kind,
                users.Length,
                hasMore,
                hasMore ? Token : null,
                Users: users);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);
    }

    private sealed class SeriesCursorEntry(IFetchEngine<Series> engine, long? userId)
        : CursorEntry(PixevalMcpCursorKind.Series, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<Series> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<Series?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            runtime.CacheUserInfos(items.Select(static series => series.User));
            var series = items.Select(PixevalSeriesDto.FromSeries).ToArray();
            return new(
                Kind,
                series.Length,
                hasMore,
                hasMore ? Token : null,
                Series: series);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);
    }

    private sealed class SpotlightCursorEntry(IFetchEngine<Spotlight> engine, long? userId)
        : CursorEntry(PixevalMcpCursorKind.Spotlight, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<Spotlight> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<Spotlight?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            var spotlights = items.Select(PixevalSpotlightDto.FromSpotlight).ToArray();
            return new(
                Kind,
                spotlights.Length,
                hasMore,
                hasMore ? Token : null,
                Spotlights: spotlights);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);
    }

    private sealed class CommentCursorEntry(IFetchEngine<Comment> engine, long? userId)
        : CursorEntry(PixevalMcpCursorKind.Comment, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<Comment> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<Comment?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            runtime.CacheUserInfos(items.Select(static comment => comment.User));
            var comments = items.Select(comment => PixevalCommentDto.FromComment(comment, runtime.CurrentUser?.Id))
                .ToArray();
            return new(
                Kind,
                comments.Length,
                hasMore,
                hasMore ? Token : null,
                Comments: comments);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);
    }

    private sealed class BookmarkTagCursorEntry(
        IFetchEngine<BookmarkTag> engine,
        long? userId,
        long bookmarkUserId,
        string workType,
        string privacy)
        : CursorEntry(PixevalMcpCursorKind.BookmarkTag, engine.EngineHandle, userId)
    {
        private readonly IAsyncEnumerator<BookmarkTag> _enumerator = engine.GetAsyncEnumerator();

        private readonly Ref<BookmarkTag?> _buffer = new();

        public override async Task<PixevalCursorPageDto> ReadPageAsync(
            IPixevalMcpRuntime runtime,
            int count,
            CancellationToken cancellationToken)
        {
            var (items, hasMore) = await ReadItemsAsync(_enumerator, count, _buffer, cancellationToken)
                .ConfigureAwait(false);
            var tags = items.Select(static tag => PixevalBookmarkTagDto.FromBookmarkTag(tag)).ToArray();
            return new(
                Kind,
                tags.Length,
                hasMore,
                hasMore ? Token : null,
                BookmarkTags: tags,
                UserId: bookmarkUserId,
                WorkType: workType,
                Privacy: privacy);
        }

        public override ValueTask DisposeAsync() => DisposeCoreAsync(_enumerator);
    }

    private sealed class Ref<T>
    {
        public T? Value { get; set; }
    }
}
