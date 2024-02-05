#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/SpotlightArticleEngine.cs
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
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class SpotlightArticleEngine(MakoClient makoClient, EngineHandle? engineHandle) : AbstractPixivFetchEngine<SpotlightArticle>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<SpotlightArticle> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new SpotlightArticleAsyncEnumerator(this, MakoApiKind.AppApi)!;
    }

    private class SpotlightArticleAsyncEnumerator(SpotlightArticleEngine pixivFetchEngine, MakoApiKind makoApiKind)
        : RecursivePixivAsyncEnumerator<SpotlightArticle, PixivSpotlightResponse, SpotlightArticleEngine>(pixivFetchEngine, makoApiKind)
    {
        protected override bool ValidateResponse(PixivSpotlightResponse rawEntity)
        {
            return rawEntity.SpotlightArticles.IsNotNullOrEmpty();
        }

        protected override string? NextUrl(PixivSpotlightResponse? rawEntity)
        {
            return rawEntity?.NextUrl;
        }

        protected override string InitialUrl()
        {
            return "/v1/spotlight/articles?category=all";
        }

        protected override IEnumerator<SpotlightArticle>? GetNewEnumerator(PixivSpotlightResponse? rawEntity)
        {
            return (rawEntity?.SpotlightArticles as IEnumerable<SpotlightArticle>)?.GetEnumerator();
        }
    }
}
