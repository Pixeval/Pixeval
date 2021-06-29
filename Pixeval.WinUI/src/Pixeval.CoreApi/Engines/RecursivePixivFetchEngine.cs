using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// 一个可以不停的搜索新页面直到不再有更多页面可以被抓取的迭代器，一个页面可以包含多个搜索结果
    /// </summary>
    /// <typeparam name="TEntity">搜索结果对应的实体类</typeparam>
    /// <typeparam name="TRawEntity">页面对应的实体类</typeparam>
    /// <typeparam name="TFetchEngine">搜索引擎</typeparam>
    internal abstract class RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine> : AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>
        where TEntity : class?
        where TFetchEngine : class, IFetchEngine<TEntity>
    {
        private TRawEntity? Entity { get; set; }

        protected RecursivePixivAsyncEnumerator(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
            : base(pixivFetchEngine, makoApiKind)
        {
        }

        /// <summary>
        /// 获取下一个页面的URL
        /// </summary>
        /// <returns>下一个页面的URL</returns>
        protected abstract string? NextUrl(TRawEntity? rawEntity);

        /// <summary>
        /// 获取第一个页面的URL
        /// </summary>
        /// <returns>第一个页面的URL</returns>
        protected abstract string InitialUrl();

        /// <summary>
        /// 从新页面的返回结果中获取该页面的所有搜索结果的迭代器
        /// </summary>
        /// <returns>所有搜索结果的迭代器</returns>
        protected abstract Task<IEnumerator<TEntity>?> GetNewEnumeratorAsync(TRawEntity? rawEntity);

        /// <summary>
        /// 指示是否还有下一页
        /// </summary>
        /// <returns>是否还有下一页</returns>
        protected virtual bool HasNextPage() => NextUrl(Entity).IsNotNullOrEmpty();

        /// <summary>
        /// 指示是否还有下一个结果，该函数在搜索结果数被限制的时候很有用
        /// </summary>
        /// <returns>是否还有下一个结果</returns>
        protected virtual bool HasNext() => true;

        /// <summary>
        /// 获取下一个搜索结果，如果已经到达了当前页的末尾，则请求一个新的页面并返回新页面的第一个搜索结果
        /// </summary>
        /// <remarks>
        /// 如果该函数发现搜索引擎已经搜索到了末尾，也即无法再提供更多的结果，则将会设置<see cref="EngineHandle.IsCompleted"/>属性
        /// 以标明该搜索引擎已经结束运行
        /// </remarks>
        /// <returns>是否还有更多的结果</returns>
        public override async ValueTask<bool> MoveNextAsync()
        {
            if (IsCancellationRequested || !HasNext())
            {
                PixivFetchEngine.EngineHandle.Complete(); // Set the state of the 'PixivFetchEngine' to Completed
                return false;
            }

            if (Entity is null)
            {
                var first = InitialUrl();
                switch (await GetJsonResponse(first))
                {
                    case Result<TRawEntity>.Success (var raw):
                        await Update(raw);
                        break;
                    case Result<TRawEntity>.Failure (var exception):
                        if (exception is { } e)
                        {
                            throw e;
                        }
                        PixivFetchEngine.EngineHandle.Complete();
                        return false;
                }
            }

            if (CurrentEntityEnumerator!.MoveNext()) // If the enumerator can proceeds then return true
            {
                TryCacheCurrent(); // Cache if allowed in session
                return true;
            }

            if (!HasNextPage() || !HasNext()) // Check if there are more pages, return false if not
            {
                PixivFetchEngine.EngineHandle.Complete();
                return false;
            }

            if (await GetJsonResponse(NextUrl(Entity)!) is Result<TRawEntity>.Success (var value)) // Else request a new page
            {
                await Update(value);
                TryCacheCurrent();
                return true;
            }

            PixivFetchEngine.EngineHandle.Complete();
            return false;
        }

        private void TryCacheCurrent()
        {
            if (PixivFetchEngine.MakoClient.Configuration.AllowCache)
            {
                PixivFetchEngine.EngineHandle.CacheValue(Current);
            }
        }

        /// <summary>
        /// 每申请一个新的页面后负责更新迭代器，实体对象和页数
        /// </summary>
        /// <param name="rawEntity">新页面的请求结果</param>
        private async Task Update(TRawEntity rawEntity)
        {
            Entity = rawEntity;
            CurrentEntityEnumerator = await GetNewEnumeratorAsync(rawEntity) ?? EmptyEnumerators<TEntity>.Sync;
            PixivFetchEngine!.RequestedPages++;
        }
    }

    internal static class RecursivePixivAsyncEnumerators
    {
        public abstract class User<TFetchEngine> : RecursivePixivAsyncEnumerator<User, PixivUserResponse, TFetchEngine> 
            where TFetchEngine : class, IFetchEngine<User>
        {
            protected User([NotNull] TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }

            protected override bool ValidateResponse(PixivUserResponse rawEntity)
            {
                return rawEntity.Users.IsNotNullOrEmpty();
            }

            protected override string? NextUrl(PixivUserResponse? rawEntity)
            {
                return rawEntity?.NextUrl;
            }

            protected abstract override string InitialUrl();

            protected override async Task<IEnumerator<User>?> GetNewEnumeratorAsync(PixivUserResponse? rawEntity)
            {
                var tasks = rawEntity?.Users?.SelectNotNull( // wow... tough code :) 
                    u => u.UserInfo,
                    async u => await MakoClient.GetUserFromIdAsync(u.UserInfo!.Id.ToString(), TargetFilter.ForAndroid) with
                    {
                        Thumbnails = u.Illusts?.Take(3).Select(illust => illust.ToIllustration(MakoClient))
                    });
                return (await Task.WhenAll(tasks ?? Enumerable.Empty<Task<User>>()) as IEnumerable<User>).GetEnumerator();
            }

            public static User<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
            {
                return new UserImpl<TFetchEngine>(engine, kind, initialUrlFactory);
            }
        }

        private class UserImpl<TFetchEngine> : User<TFetchEngine> 
            where TFetchEngine : class, IFetchEngine<User>
        {
            private readonly Func<TFetchEngine, string> _initialUrlFactory;
            
            public UserImpl([NotNull] TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
            {
                _initialUrlFactory = initialUrlFactory;
            }

            protected override string InitialUrl()
            {
                return _initialUrlFactory(PixivFetchEngine);
            }
        }

        public abstract class Illustration<TFetchEngine> : RecursivePixivAsyncEnumerator<Illustration, PixivResponse, TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Illustration>
        {
            protected Illustration([NotNull] TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }
            
            protected override bool ValidateResponse(PixivResponse rawEntity)
            {
                return rawEntity.Illusts.IsNotNullOrEmpty();
            }

            protected override string? NextUrl(PixivResponse? rawEntity)
            {
                return rawEntity?.NextUrl;
            }

            protected abstract override string InitialUrl();

            protected override Task<IEnumerator<Illustration>?> GetNewEnumeratorAsync(PixivResponse? rawEntity)
            {
                return Task.FromResult(rawEntity?.Illusts?.SelectNotNull(illust => illust.ToIllustration(MakoClient)).GetEnumerator());
            }
            
            public static Illustration<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
            {
                return new IllustrationImpl<TFetchEngine>(engine, kind, initialUrlFactory);
            }
        }

        private class IllustrationImpl<TFetchEngine> : Illustration<TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Illustration>
        {
            private readonly Func<TFetchEngine, string> _initialUrlFactory;
            
            public IllustrationImpl([NotNull] TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
            {
                _initialUrlFactory = initialUrlFactory;
            }

            protected override string InitialUrl()
            {
                return _initialUrlFactory(PixivFetchEngine);
            }
        }
    }
}