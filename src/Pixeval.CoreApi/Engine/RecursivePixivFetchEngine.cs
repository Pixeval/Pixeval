#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RecursivePixivFetchEngine.cs
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
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine
{
    internal abstract class RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine> : AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>
        where TEntity : class
        where TFetchEngine : class, IFetchEngine<TEntity>
    {
        protected RecursivePixivAsyncEnumerator(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
            : base(pixivFetchEngine, makoApiKind)
        {
        }

        private TRawEntity? RawEntity { get; set; }

        protected abstract string? NextUrl(TRawEntity? rawEntity);

        protected abstract string InitialUrl();

        protected abstract IEnumerator<TEntity>? GetNewEnumerator(TRawEntity? rawEntity);

        protected virtual bool HasNextPage()
        {
            return NextUrl(RawEntity).IsNotNullOrEmpty();
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            if (IsCancellationRequested)
            {
                PixivFetchEngine.EngineHandle.Complete(); // Set the state of the 'PixivFetchEngine' to Completed
                return false;
            }

            if (RawEntity is null)
            {
                var first = InitialUrl();
                switch (await GetJsonResponseAsync(first).ConfigureAwait(false))
                {
                    case Result<TRawEntity>.Success (var raw):
                        Update(raw);
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
                return true;
            }

            if (!HasNextPage()) // Check if there are more pages, return false if not
            {
                PixivFetchEngine.EngineHandle.Complete();
                return false;
            }

            if (await GetJsonResponseAsync(NextUrl(RawEntity)!).ConfigureAwait(false) is Result<TRawEntity>.Success (var value)) // Else request a new page
            {
                if (IsCancellationRequested)
                {
                    PixivFetchEngine.EngineHandle.Complete();
                    return false;
                }

                Update(value);
                return true;
            }

            PixivFetchEngine.EngineHandle.Complete();
            return false;
        }

        private void Update(TRawEntity rawEntity)
        {
            RawEntity = rawEntity;
            CurrentEntityEnumerator = GetNewEnumerator(rawEntity) ?? EmptyEnumerators<TEntity>.Sync;
            PixivFetchEngine.RequestedPages++;
        }
    }

    internal static class RecursivePixivAsyncEnumerators
    {
        public abstract class User<TFetchEngine> : RecursivePixivAsyncEnumerator<User, PixivUserResponse, TFetchEngine>
            where TFetchEngine : class, IFetchEngine<User>
        {
            protected User(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
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

            protected override IEnumerator<User>? GetNewEnumerator(PixivUserResponse? rawEntity)
            {
                var tasks = rawEntity?.Users;
                return tasks?.GetEnumerator();
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

            public UserImpl(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
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
            protected Illustration(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
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

            protected override IEnumerator<Illustration>? GetNewEnumerator(PixivResponse? rawEntity)
            {
                return rawEntity?.Illusts?.GetEnumerator();
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

            public IllustrationImpl(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
            {
                _initialUrlFactory = initialUrlFactory;
            }

            protected override string InitialUrl()
            {
                return _initialUrlFactory(PixivFetchEngine);
            }
        }

        public abstract class Novel<TFetchEngine> : RecursivePixivAsyncEnumerator<Novel, PixivNovelResponse, TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Novel>
        {
            protected Novel(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }

            protected override bool ValidateResponse(PixivNovelResponse rawEntity)
            {
                return rawEntity.Novels.IsNotNullOrEmpty();
            }

            protected override string? NextUrl(PixivNovelResponse? rawEntity)
            {
                return rawEntity?.NextUrl;
            }

            protected abstract override string InitialUrl();

            protected override IEnumerator<Novel>? GetNewEnumerator(PixivNovelResponse? rawEntity)
            {
                return rawEntity?.Novels?.GetEnumerator();
            }

            public static Novel<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
            {
                return new NovelImpl<TFetchEngine>(engine, kind, initialUrlFactory);
            }
        }

        private class NovelImpl<TFetchEngine> : Novel<TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Novel>
        {
            private readonly Func<TFetchEngine, string> _initialUrlFactory;

            public NovelImpl(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
            {
                _initialUrlFactory = initialUrlFactory;
            }

            protected override string InitialUrl()
            {
                return _initialUrlFactory(PixivFetchEngine);
            }
        }

        public abstract class Comment<TFetchEngine> : RecursivePixivAsyncEnumerator<Comment, IllustrationCommentsResponse, TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Comment>
        {
            protected Comment(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }

            protected override bool ValidateResponse(IllustrationCommentsResponse rawEntity)
            {
                return rawEntity.Comments.IsNotNullOrEmpty();
            }

            protected override string? NextUrl(IllustrationCommentsResponse? rawEntity)
            {
                return rawEntity?.NextUrl;
            }

            protected abstract override string InitialUrl();

            protected override IEnumerator<Comment>? GetNewEnumerator(IllustrationCommentsResponse? rawEntity)
            {
                return rawEntity?.Comments?.GetEnumerator();
            }

            public static Comment<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
            {
                return new CommentImpl<TFetchEngine>(engine, kind, initialUrlFactory);
            }
        }

        private class CommentImpl<TFetchEngine> : Comment<TFetchEngine>
            where TFetchEngine : class, IFetchEngine<Comment>
        {
            private readonly Func<TFetchEngine, string> _initialUrlFactory;

            public CommentImpl(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory) : base(pixivFetchEngine, makoApiKind)
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