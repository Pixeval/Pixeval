#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/RecursivePixivFetchEngine.cs
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

namespace Pixeval.CoreApi.Engine;

internal abstract class RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine,
    MakoApiKind makoApiKind) : AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(pixivFetchEngine,
    makoApiKind)
    where TFetchEngine : class, IFetchEngine<TEntity>
{
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
                case Result<TRawEntity>.Success(var raw):
                    Update(raw);
                    break;
                case Result<TRawEntity>.Failure(var exception):
                    if (exception is not null)
                    {
                        MakoClient.Logger.LogError("", exception);
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

        if (await GetJsonResponseAsync(NextUrl(RawEntity)!).ConfigureAwait(false) is Result<TRawEntity>.Success(var value)) // Else request a new page
        {
            if (IsCancellationRequested)
            {
                PixivFetchEngine.EngineHandle.Complete();
                return false;
            }

            Update(value);
            _ = CurrentEntityEnumerator.MoveNext();
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
    public abstract class User<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) :
        RecursivePixivAsyncEnumerator<User, PixivUserResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<User>
    {
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
            return (rawEntity?.Users as IEnumerable<User>)?.GetEnumerator();
        }

        public static User<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new UserImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class UserImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind,
            Func<TFetchEngine, string> initialUrlFactory)
        : User<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<User>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Illustration<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) :
        RecursivePixivAsyncEnumerator<Illustration, PixivResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Illustration>
    {
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
            return (rawEntity?.Illusts as IEnumerable<Illustration>)?.GetEnumerator();
        }

        public static Illustration<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new IllustrationImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class IllustrationImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind,
            Func<TFetchEngine, string> initialUrlFactory)
        : Illustration<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Illustration>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Novel<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) :
        RecursivePixivAsyncEnumerator<Novel, PixivNovelResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Novel>
    {
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
            return (rawEntity?.Novels as IEnumerable<Novel>)?.GetEnumerator();
        }

        public static Novel<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new NovelImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class NovelImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind,
            Func<TFetchEngine, string> initialUrlFactory)
        : Novel<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Novel>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Comment<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind) :
        RecursivePixivAsyncEnumerator<Comment, IllustrationCommentsResponse, TFetchEngine>(pixivFetchEngine,
            makoApiKind)
        where TFetchEngine : class, IFetchEngine<Comment>
    {
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
            return (rawEntity?.Comments as IEnumerable<Comment>)?.GetEnumerator();
        }

        public static Comment<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new CommentImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class CommentImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind,
            Func<TFetchEngine, string> initialUrlFactory)
        : Comment<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Comment>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }
}
