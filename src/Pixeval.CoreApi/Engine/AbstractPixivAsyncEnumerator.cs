#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/AbstractPixivAsyncEnumerator.cs
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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine;

/// <summary>
/// An abstract enumerator that encapsulates the required properties for Pixiv, it is intended to be
/// cooperated with the fetch engine, the fetch engine will fetch a new page(the page can be in multiple formats, such
/// as json),
/// and normally, a page can contain multiple result entries. When <see cref="MoveNextAsync" /> method is called, the
/// fetch
/// engine will try to get the next result entry in the current page, if there are no more entries, it will try to
/// fetch the next page, and if that also fails, then all the pages have been fetched, iteration is over.
/// </summary>
/// <typeparam name="TEntity">The entity that will be yield by the enumerator</typeparam>
/// <typeparam name="TRawEntity">The entity class corresponding to the result entry</typeparam>
/// <typeparam name="TFetchEngine">The fetch engine</typeparam>
public abstract class AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine>(TFetchEngine pixivFetchEngine,
    MakoApiKind apiKind) : IAsyncEnumerator<TEntity>
    where TFetchEngine : class, IFetchEngine<TEntity>
    where TEntity : class, IEntry
    where TRawEntity : class
{
    protected readonly MakoClient MakoClient = pixivFetchEngine.MakoClient;
    protected readonly TFetchEngine PixivFetchEngine = pixivFetchEngine;

    /// <summary>
    /// The result entries of the current page
    /// </summary>
    protected IEnumerator<TEntity>? CurrentEntityEnumerator;

    /// <summary>
    /// Indicates which kind of API this enumerator will use
    /// </summary>
    private MakoApiKind ApiKind { get; } = apiKind;

    /// <summary>
    /// Indicates if the current operation has been cancelled
    /// </summary>
    protected bool IsCancellationRequested => PixivFetchEngine.EngineHandle.IsCancelled;

    /// <summary>
    /// The current result entry of <see cref="CurrentEntityEnumerator" />
    /// </summary>
    public TEntity Current => CurrentEntityEnumerator?.Current!;

    /// <summary>
    /// Moves the <see cref="MoveNextAsync" /> one step ahead, if fails, it will try to
    /// fetch a new page
    /// </summary>
    /// <returns></returns>
    public abstract ValueTask<bool> MoveNextAsync();

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return default;
    }

    /// <summary>
    /// Check if the new page contains valid response
    /// </summary>
    /// <param name="rawEntity">The new page</param>
    /// <returns></returns>
    protected abstract bool ValidateResponse(TRawEntity rawEntity);

    protected async Task<Result<TRawEntity>> GetJsonResponseAsync(string url)
    {
        try
        {
            var responseMessage = await MakoClient.GetMakoHttpClient(ApiKind).GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return Result<TRawEntity>.AsFailure(await MakoNetworkException.FromHttpResponseMessageAsync(responseMessage, MakoClient.Configuration.DomainFronting).ConfigureAwait(false));
            }

            var str = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (JsonSerializer.Deserialize(str, typeof(TRawEntity), AppJsonSerializerContext.Default) is TRawEntity result)
                if (ValidateResponse(result))
                    return Result<TRawEntity>.AsSuccess(result);
            return Result<TRawEntity>.AsFailure();
        }
        catch (Exception e)
        {
            return Result<TRawEntity>.AsFailure(new MakoNetworkException(url, MakoClient.Configuration.DomainFronting, e.Message, (int?)(e as HttpRequestException)?.StatusCode ?? -1));
        }
    }

    protected async Task<List<Result<TRawEntity>>> GetJsonResponsesAsync(List<string> urls)
    {
        var tasks = new List<Task<Result<TRawEntity>>>();
        foreach (var url in urls)
        {
            tasks.Add(GetJsonResponseAsync(url));
        }
        return new List<Result<TRawEntity>>(await Task.WhenAll(tasks).ConfigureAwait(false));
    }
    }
