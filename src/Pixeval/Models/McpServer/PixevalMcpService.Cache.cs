// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;
using Mako.Net.Responses;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService
{
    private const int MaxCachedWorkCount = 512;
    private const int MaxCachedUserCount = 512;

    private readonly Lock _cacheLock = new();
    private readonly Dictionary<(SimpleWorkType Type, long Id), WorkBase> _workCache = [];
    private readonly Queue<(SimpleWorkType Type, long Id)> _workCacheOrder = [];
    private readonly Dictionary<long, UserBasicInfo> _userInfoCache = [];
    private readonly Queue<long> _userInfoCacheOrder = [];
    private readonly Dictionary<long, SingleUserResponse> _singleUserCache = [];
    private readonly Queue<long> _singleUserCacheOrder = [];

    public void CacheWorks(IEnumerable<WorkBase> works)
    {
        lock (_cacheLock)
        {
            foreach (var work in works)
                AddWorkCore(work);
        }
    }

    public void CacheUsers(IEnumerable<User> users)
    {
        lock (_cacheLock)
        {
            foreach (var user in users)
            {
                AddUserInfoCore(user.UserInfo);
                foreach (var illustration in user.Illustrations)
                    AddWorkCore(illustration);
                foreach (var novel in user.Novels)
                    AddWorkCore(novel);
            }
        }
    }

    public void CacheUserInfos(IEnumerable<UserBasicInfo> users)
    {
        lock (_cacheLock)
        {
            foreach (var user in users)
                AddUserInfoCore(user);
        }
    }

    public async Task<WorkBase> GetWorkAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken cancellationToken) =>
        workType switch
        {
            SimpleWorkType.Illustration => await GetIllustrationAsync(id, cancellationToken).ConfigureAwait(false),
            SimpleWorkType.Novel => await GetNovelAsync(id, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(workType))
        };

    public async Task<Illustration> GetIllustrationAsync(long id, CancellationToken cancellationToken)
    {
        if (TryGetCachedWork(SimpleWorkType.Illustration, id) is Illustration cached)
            return cached;

        var illustration = await MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        CacheWorks([illustration]);
        return illustration;
    }

    public async Task<Novel> GetNovelAsync(long id, CancellationToken cancellationToken)
    {
        if (TryGetCachedWork(SimpleWorkType.Novel, id) is Novel cached)
            return cached;

        var novel = await MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
        CacheWorks([novel]);
        return novel;
    }

    public async Task<SingleUserResponse> GetUserAsync(long id, CancellationToken cancellationToken)
    {
        if (TryGetCachedSingleUser(id) is { } cached)
            return cached;

        var user = await MakoClient.GetUserFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
        CacheSingleUser(user);
        return user;
    }

    public async Task<UserBasicInfo> GetUserBasicInfoAsync(long id, CancellationToken cancellationToken)
    {
        if (TryGetCachedUserInfo(id) is { } cached)
            return cached;

        return (await GetUserAsync(id, cancellationToken).ConfigureAwait(false)).UserEntity;
    }

    private WorkBase? TryGetCachedWork(SimpleWorkType type, long id)
    {
        lock (_cacheLock)
            return _workCache.GetValueOrDefault((type, id));
    }

    private UserBasicInfo? TryGetCachedUserInfo(long id)
    {
        lock (_cacheLock)
            return _userInfoCache.GetValueOrDefault(id);
    }

    private SingleUserResponse? TryGetCachedSingleUser(long id)
    {
        lock (_cacheLock)
            return _singleUserCache.GetValueOrDefault(id);
    }

    private void CacheSingleUser(SingleUserResponse user)
    {
        lock (_cacheLock)
        {
            AddSingleUserCore(user);
            AddUserInfoCore(user.UserEntity);
        }
    }

    private void AddWorkCore(WorkBase work)
    {
        var key = (ToSimpleWorkType(work), work.Id);
        if (!_workCache.ContainsKey(key))
            _workCacheOrder.Enqueue(key);

        _workCache[key] = work;
        AddUserInfoCore(work.User);
        TrimCache(_workCache, _workCacheOrder, MaxCachedWorkCount);
    }

    private void AddUserInfoCore(UserBasicInfo user)
    {
        if (!_userInfoCache.ContainsKey(user.Id))
            _userInfoCacheOrder.Enqueue(user.Id);

        _userInfoCache[user.Id] = user;
        TrimCache(_userInfoCache, _userInfoCacheOrder, MaxCachedUserCount);
    }

    private void AddSingleUserCore(SingleUserResponse user)
    {
        var id = user.UserEntity.Id;
        if (!_singleUserCache.ContainsKey(id))
            _singleUserCacheOrder.Enqueue(id);

        _singleUserCache[id] = user;
        TrimCache(_singleUserCache, _singleUserCacheOrder, MaxCachedUserCount);
    }

    private void UpdateCachedWork(
        SimpleWorkType type,
        long id,
        Action<WorkBase> update)
    {
        lock (_cacheLock)
        {
            if (_workCache.TryGetValue((type, id), out var work))
                update(work);
        }
    }

    private PixevalWorkDto? GetCachedWorkDto(SimpleWorkType type, long id)
    {
        lock (_cacheLock)
            return _workCache.TryGetValue((type, id), out var work)
                ? PixevalWorkDto.FromWork(work)
                : null;
    }

    private static void TrimCache<TKey, TValue>(
        Dictionary<TKey, TValue> cache,
        Queue<TKey> order,
        int maxCount)
        where TKey : notnull
    {
        while (cache.Count > maxCount && order.TryDequeue(out var key))
            _ = cache.Remove(key);
    }

    private static SimpleWorkType ToSimpleWorkType(WorkBase work) =>
        work switch
        {
            Illustration => SimpleWorkType.Illustration,
            Novel => SimpleWorkType.Novel,
            _ => throw new ArgumentOutOfRangeException(nameof(work), work.GetType().FullName)
        };
}
