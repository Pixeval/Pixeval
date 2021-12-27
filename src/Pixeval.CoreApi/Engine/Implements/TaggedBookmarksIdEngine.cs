#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/TaggedBookmarksIdEngine.cs
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
using System.Threading;
using System.Web;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

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