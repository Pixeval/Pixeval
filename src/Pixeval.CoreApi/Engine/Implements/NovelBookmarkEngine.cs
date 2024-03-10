#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/NovelBookmarkEngine.cs
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
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

public class NovelBookmarkEngine(
    MakoClient makoClient,
    long uid,
    string? tag,
    PrivacyPolicy privacyPolicy,
    TargetFilter targetFilter,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    private readonly string? _tag = tag;
    private readonly PrivacyPolicy _privacyPolicy = privacyPolicy;
    private readonly TargetFilter _targetFilter = targetFilter;
    private readonly long _uid = uid;

    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        return RecursivePixivAsyncEnumerators.Novel<NovelBookmarkEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
            engine =>
            {
                var tagSegment = engine._tag is not null ? $"&tag={HttpUtility.UrlEncode(engine._tag)}" : null;
                return "/v1/user/bookmarks/novel"
                       + $"?user_id={engine._uid}"
                       + $"&restrict={engine._privacyPolicy.GetDescription()}"
                       + $"&filter={engine._targetFilter.GetDescription()}"
                       + tagSegment;
            })!;
    }
}
