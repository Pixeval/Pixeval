#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/SpotlightArticleEngine.cs
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
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements
{
    internal class SpotlightArticleEngine : AbstractPixivFetchEngine<SpotlightArticle>
    {
        public SpotlightArticleEngine(MakoClient makoClient, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
        }

        public override IAsyncEnumerator<SpotlightArticle> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return new SpotlightArticleAsyncEnumerator(this, MakoApiKind.AppApi)!;
        }

        private class SpotlightArticleAsyncEnumerator : RecursivePixivAsyncEnumerator<SpotlightArticle, PixivSpotlightResponse, SpotlightArticleEngine>
        {
            public SpotlightArticleAsyncEnumerator(SpotlightArticleEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }

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
                return rawEntity?.SpotlightArticles?.GetEnumerator();
            }
        }
    }
}