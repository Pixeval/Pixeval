#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/PostedNovelEngine.cs
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
using JetBrains.Annotations;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engine.Implements
{
    public class PostedNovelEngine : AbstractPixivFetchEngine<Novel>
    {
        private readonly TargetFilter _targetFilter;
        private readonly string _uid;

        public PostedNovelEngine(
            [NotNull] MakoClient makoClient,
            string uid,
            TargetFilter targetFilter,
            EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _uid = uid;
            _targetFilter = targetFilter;
        }

        public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Novel<PostedNovelEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => "/v1/user/novels"
                          + $"?user_id={engine._uid}"
                          + $"&filter={engine._targetFilter.GetDescription()}")!;
        }
    }
}