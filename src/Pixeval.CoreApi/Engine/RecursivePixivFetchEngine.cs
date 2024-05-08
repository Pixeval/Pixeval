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

using System.Collections.Generic;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine;

internal abstract class RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind)
    : AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(pixivFetchEngine, makoApiKind)
    where TEntity : class, IEntry 
    where TRawEntity : class
    where TFetchEngine : class, IFetchEngine<TEntity> 
{
    private TRawEntity? RawEntity { get; set; }

    protected abstract string? NextUrl(TRawEntity? rawEntity);

    protected abstract string InitialUrl { get; }

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
            switch (await GetJsonResponseAsync(InitialUrl).ConfigureAwait(false))
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
        ++PixivFetchEngine.RequestedPages;
    }
}

internal static class RecursivePixivAsyncEnumerators
{
    public abstract class BaseRecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine, MakoApiKind makoApiKind, string initialUrl)
        : RecursivePixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(pixivFetchEngine, makoApiKind)
        where TEntity : class, IEntry 
        where TRawEntity : PixivNextUrlResponse<TEntity>
        where TFetchEngine : class, IFetchEngine<TEntity> 
    {
        protected override string InitialUrl => initialUrl;

        protected sealed override bool ValidateResponse(TRawEntity rawEntity) => rawEntity.Entities.IsNotNullOrEmpty();

        protected sealed override string? NextUrl(TRawEntity? rawEntity) => rawEntity?.NextUrl;

        protected sealed override IEnumerator<TEntity>? GetNewEnumerator(TRawEntity? rawEntity) => (rawEntity?.Entities as IEnumerable<TEntity>)?.GetEnumerator();
    }

    public class User<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<User, PixivUserResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<User>;

    public class Illustration<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<Illustration, PixivIllustrationResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<Illustration>;

    public class Novel<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<Novel, PixivNovelResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<Novel>;

    public class Comment<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<Comment, PixivCommentResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<Comment>;

    public class BookmarkTag<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<BookmarkTag, PixivBookmarkTagResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<BookmarkTag>;

    public class Spotlight<TFetchEngine>(TFetchEngine pixivFetchEngine, string initialUrl)
        : BaseRecursivePixivAsyncEnumerator<Spotlight, PixivSpotlightResponse, TFetchEngine>(pixivFetchEngine, MakoApiKind.AppApi, initialUrl)
        where TFetchEngine : class, IFetchEngine<Spotlight>;
}
