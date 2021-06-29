using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JetBrains.Annotations;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    /// <summary>
    /// Get the bookmarks that have user-defined tags associate with them, only returns their ID in string representation
    /// This API is not supposed to have other usages
    /// </summary>
    internal class TaggedBookmarksIdEngine : AbstractPixivFetchEngine<string>
    {
        private readonly string _uid;
        private readonly string _tag;
        
        public TaggedBookmarksIdEngine([NotNull] MakoClient makoClient, EngineHandle? engineHandle, string uid, string tag) : base(makoClient, engineHandle)
        {
            _uid = uid;
            _tag = tag;
        }

        public override IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return new TaggedBookmarksIdAsyncEnumerator(this, MakoApiKind.WebApi)!;
        }

        private class TaggedBookmarksIdAsyncEnumerator : RecursivePixivAsyncEnumerator<string, WebApiBookmarksWithTagResponse, TaggedBookmarksIdEngine>
        {
            private int _currentIndex;
            
            public TaggedBookmarksIdAsyncEnumerator([NotNull] TaggedBookmarksIdEngine pixivFetchEngine, MakoApiKind apiKind) : base(pixivFetchEngine, apiKind)
            {
            }

            protected override bool ValidateResponse(WebApiBookmarksWithTagResponse rawEntity)
            {
                return rawEntity.ResponseBody?.Works.IsNotNullOrEmpty() ?? false;
            }

            protected override string NextUrl(WebApiBookmarksWithTagResponse? rawEntity) => GetUrl();

            protected override string InitialUrl() => GetUrl();

            protected override Task<IEnumerator<string>?> GetNewEnumeratorAsync(WebApiBookmarksWithTagResponse? rawEntity)
            {
                _currentIndex++; // Cannot put it in the GetUrl() because the NextUrl() gonna be called twice at each iteration which will increases the _currentIndex by 2
                return Task.FromResult(rawEntity?.ResponseBody?.Works?.SelectNotNull(w => w.Id, w => w.Id!).GetEnumerator());
            }
            
            private string GetUrl()
            {
                return $"/ajax/user/{PixivFetchEngine._uid}/illusts/bookmarks?tag={HttpUtility.UrlEncode(PixivFetchEngine._tag)}&offset={_currentIndex * 100}&limit=100&rest=show&lang=";
            } 
        }
    }
}