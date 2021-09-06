#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/AbstractPixivAsyncEnumerator.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engine
{
    /// <summary>
    ///     An abstract enumerator that encapsulates the required properties for Pixiv, it is intended to be
    ///     cooperated with the fetch engine, the fetch engine will fetch a new page(the page can be in multiple formats, such
    ///     as json),
    ///     and normally, a page can contain multiple result entries. When <see cref="MoveNextAsync" /> method is called, the
    ///     fetch
    ///     engine will tries to get the next result entry in the current page, if there are no more entries, it will tries to
    ///     fetch the next page, and if that also fails, then all of the pages have been fetched, iteration is over.
    /// </summary>
    /// <typeparam name="TEntity">The entity that will be yield by the enumerator</typeparam>
    /// <typeparam name="TRawEntity">The entity class corresponding to the result entry</typeparam>
    /// <typeparam name="TFetchEngine">The fetch engine</typeparam>
    public abstract class AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine> : IAsyncEnumerator<TEntity?>
        where TEntity : class?
        where TFetchEngine : class, IFetchEngine<TEntity>
    {
        protected readonly MakoClient MakoClient;
        protected readonly TFetchEngine PixivFetchEngine;

        /// <summary>
        ///     The result entries of the current page
        /// </summary>
        protected IEnumerator<TEntity>? CurrentEntityEnumerator;

        protected AbstractPixivAsyncEnumerator(TFetchEngine pixivFetchEngine, MakoApiKind apiKind)
        {
            PixivFetchEngine = pixivFetchEngine;
            MakoClient = pixivFetchEngine.MakoClient;
            ApiKind = apiKind;
        }

        /// <summary>
        ///     Indicates which kind of API this enumerator will use
        /// </summary>
        private MakoApiKind ApiKind { get; }

        /// <summary>
        ///     Indicates if the current operation has been cancelled
        /// </summary>
        protected bool IsCancellationRequested => PixivFetchEngine.EngineHandle.CancellationTokenSource.IsCancellationRequested;

        /// <summary>
        ///     The current result entry of <see cref="CurrentEntityEnumerator" />
        /// </summary>
        public TEntity? Current => CurrentEntityEnumerator?.Current;

        /// <summary>
        ///     Moves the <see cref="MoveNextAsync" /> one step ahead, if fails, it will tries to
        ///     fetch a new page
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask<bool> MoveNextAsync();

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return default;
        }

        /// <summary>
        ///     Check if the new page contains valid response
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
                    return Result<TRawEntity>.OfFailure(await MakoNetworkException.FromHttpResponseMessageAsync(responseMessage, MakoClient.Configuration.Bypass).ConfigureAwait(false));
                }

                var result = (await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false)).FromJson<TRawEntity>();
                if (result is null)
                {
                    return Result<TRawEntity>.OfFailure();
                }

                return ValidateResponse(result)
                    ? Result<TRawEntity>.OfSuccess(result)
                    : Result<TRawEntity>.OfFailure();
            }
            catch (HttpRequestException e)
            {
                return Result<TRawEntity>.OfFailure(new MakoNetworkException(url, MakoClient.Configuration.Bypass, e.Message, (int?) e.StatusCode ?? -1));
            }
        }
    }
}