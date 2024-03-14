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

internal abstract class RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
    : AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(pixivFetchEngine, makoApiKind)
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
                        MakoClient.LogException(exception);
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
    public abstract class BaseRecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<TEntity> where TEntity : class, IEntry where TRawEntity : PixivNextUrlResponse<TEntity>
    {
        protected sealed override bool ValidateResponse(TRawEntity rawEntity)
        {
            return rawEntity.Entities.IsNotNullOrEmpty();
        }

        protected sealed override string? NextUrl(TRawEntity? rawEntity)
        {
            return rawEntity?.NextUrl;
        }

        protected sealed override IEnumerator<TEntity>? GetNewEnumerator(TRawEntity? rawEntity)
        {
            return (rawEntity?.Entities as IEnumerable<TEntity>)?.GetEnumerator();
        }
    }

    public abstract class User<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : BaseRecursivePixivAsyncEnumerator<User, PixivUserResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<User>
    {
        public static User<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new UserImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class UserImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory)
        : User<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<User>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Illustration<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : BaseRecursivePixivAsyncEnumerator<Illustration, PixivIllustrationResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Illustration>
    {
        public static Illustration<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new IllustrationImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class IllustrationImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory)
        : Illustration<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Illustration>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Novel<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : BaseRecursivePixivAsyncEnumerator<Novel, PixivNovelResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Novel>
    {
        public static Novel<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new NovelImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class NovelImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory)
        : Novel<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Novel>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class Comment<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : BaseRecursivePixivAsyncEnumerator<Comment, PixivCommentResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Comment>
    {
        public static Comment<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new CommentImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class CommentImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory)
        : Comment<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<Comment>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }

    public abstract class BookmarkTag<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : BaseRecursivePixivAsyncEnumerator<BookmarkTag, PixivBookmarkTagResponse, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<BookmarkTag>
    {
        public static BookmarkTag<TFetchEngine> WithInitialUrl(TFetchEngine engine, MakoApiKind kind, Func<TFetchEngine, string> initialUrlFactory)
        {
            return new BookmarkTagImpl<TFetchEngine>(engine, kind, initialUrlFactory);
        }
    }

    private class BookmarkTagImpl<TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, Func<TFetchEngine, string> initialUrlFactory)
        : BookmarkTag<TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TFetchEngine : class, IFetchEngine<BookmarkTag>
    {
        protected override string InitialUrl()
        {
            return initialUrlFactory(PixivFetchEngine);
        }
    }
}
