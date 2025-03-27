// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using System.Web;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

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
