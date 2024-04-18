#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/BookmarkEngine.cs
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
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

/// <summary>
/// An <see cref="IFetchEngine{E}" /> that fetches the bookmark of a specific user
/// </summary>
/// <remarks>
/// Creates a <see cref="IllustrationBookmarkEngine" />
/// </remarks>
/// <param name="makoClient">The <see cref="MakoClient" /> that owns this object</param>
/// <param name="uid">Id of the user</param>
/// <param name="privacyPolicy">The privacy option</param>
/// <param name="targetFilter">Indicates the target API of the fetch operation</param>
/// <param name="engineHandle"></param>
internal class IllustrationBookmarkEngine(
    MakoClient makoClient,
    long uid,
    string? tag,
    PrivacyPolicy privacyPolicy,
    TargetFilter targetFilter,
    EngineHandle? engineHandle = null) 
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<IllustrationBookmarkEngine>(
            this,
            "/v1/user/bookmarks/illust"
            + $"?user_id={uid}"
            + $"&restrict={privacyPolicy.GetDescription()}"
            + $"&filter={targetFilter.GetDescription()}"
            + tag?.Let(s => $"&tag={HttpUtility.UrlEncode(s)}"));
}
