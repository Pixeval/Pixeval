#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/TaggedBookmarksIdEngine.cs
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

using System.Collections.Generic;
using System.Threading;
using System.Web;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements
{
    /// <summary>
    ///     Get the bookmarks that have user-defined tags associate with them, only returns their ID in string representation
    ///     This API is not supposed to have other usages
    /// </summary>
    internal class TaggedBookmarksIdEngine : AbstractPixivFetchEngine<string>
    {
        private readonly string _tag;
        private readonly string _uid;

        public TaggedBookmarksIdEngine(MakoClient makoClient, EngineHandle? engineHandle, string uid, string tag) : base(makoClient, engineHandle)
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

            public TaggedBookmarksIdAsyncEnumerator(TaggedBookmarksIdEngine pixivFetchEngine, MakoApiKind apiKind) : base(pixivFetchEngine, apiKind)
            {
            }

            protected override bool ValidateResponse(WebApiBookmarksWithTagResponse rawEntity)
            {
                return rawEntity.ResponseBody?.Works.IsNotNullOrEmpty() ?? false;
            }

            protected override string NextUrl(WebApiBookmarksWithTagResponse? rawEntity)
            {
                return GetUrl();
            }

            protected override string InitialUrl()
            {
                return GetUrl();
            }

            protected override IEnumerator<string>? GetNewEnumerator(WebApiBookmarksWithTagResponse? rawEntity)
            {
                _currentIndex++; // Cannot put it in the GetUrl() because the NextUrl() gonna be called twice at each iteration which will increases the _currentIndex by 2
                return rawEntity?.ResponseBody?.Works?.SelectNotNull(w => w.Id, w => w.Id!).GetEnumerator();
            }

            private string GetUrl()
            {
                return $"/ajax/user/{PixivFetchEngine._uid}/illusts/bookmarks?tag={HttpUtility.UrlEncode(PixivFetchEngine._tag)}&offset={_currentIndex * 100}&limit=100&rest=show&lang=";
            }
        }
    }
}