using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// An abstract enumerator that encapsulate the required properties for Pixiv At each iteration, and intended to be cooperated
    /// with the fetch engine, the fetch engine will request a new page(the page can be in multiple formats, such as json), and
    /// normally, a page can contain multiple result entries. When <see cref="MoveNextAsync"/> method is called, the fetch engine
    /// will tries to get the next result entry in the current page, if there have no more entries, it will tries to request the next
    /// page, and if that also fails, it means that all of the pages have been fetched, iteration is over.
    /// </summary>
    /// <typeparam name="TEntity">The entity that will be yield by the enumerator</typeparam>
    /// <typeparam name="TRawEntity">The entity class corresponding to the result entry</typeparam>
    /// <typeparam name="TFetchEngine">The fetch engine</typeparam>
    public abstract class AbstractPixivAsyncEnumerator<TEntity, TRawEntity, TFetchEngine> : IAsyncEnumerator<TEntity?>
        where TEntity : class?
        where TFetchEngine : class, IFetchEngine<TEntity>
    {
        protected readonly TFetchEngine PixivFetchEngine;
        
        /// <summary>
        /// The result entries of the current page
        /// </summary>
        protected IEnumerator<TEntity>? CurrentEntityEnumerator;
        
        /// <summary>
        /// The current result entry of <see cref="CurrentEntityEnumerator"/>
        /// </summary>
        public TEntity? Current => CurrentEntityEnumerator?.Current;

        /// <summary>
        /// Indicates which kind of API this enumerator will use
        /// </summary>
        private MakoApiKind ApiKind { get; }
        
        protected readonly MakoClient MakoClient;
        
        /// <summary>
        /// Indicates if the current operation has been cancelled
        /// </summary>
        protected bool IsCancellationRequested => PixivFetchEngine.EngineHandle.CancellationTokenSource.IsCancellationRequested;

        protected AbstractPixivAsyncEnumerator(TFetchEngine pixivFetchEngine, MakoApiKind apiKind)
        {
            PixivFetchEngine = pixivFetchEngine;
            MakoClient = pixivFetchEngine.MakoClient;
            ApiKind = apiKind;
        }

        /// <summary>
        /// Moves the <see cref="MoveNextAsync"/> one step ahead, if fails, it will tries to
        /// fetch a new page
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask<bool> MoveNextAsync();

        /// <summary>
        /// Check if the new page contains valid response
        /// </summary>
        /// <param name="rawEntity">The new page</param>
        /// <returns></returns>
        protected abstract bool ValidateResponse(TRawEntity rawEntity);
        
        protected async Task<Result<TRawEntity>> GetJsonResponse(string url)
        {
            try
            {
                var responseMessage = await MakoClient.GetMakoHttpClient(ApiKind).GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return Result<TRawEntity>.OfFailure(await MakoNetworkException.FromHttpResponseMessage(responseMessage, MakoClient.Configuration.Bypass));
                }

                var result = (await responseMessage.Content.ReadAsStringAsync()).FromJson<TRawEntity>();
                if (result is null) return Result<TRawEntity>.OfFailure();
                return ValidateResponse(result)
                    ? Result<TRawEntity>.OfSuccess(result)
                    : Result<TRawEntity>.OfFailure();
            }
            catch (HttpRequestException e)
            {
                return Result<TRawEntity>.OfFailure(new MakoNetworkException(url, MakoClient.Configuration.Bypass, e.Message, (int?) e.StatusCode ?? -1));
            }
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);   
            return default;
        }
    }
}