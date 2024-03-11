#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/RecentPostedIllustrationEngine.cs
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
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

public class RecentPostedIllustrationEngine(MakoClient makoClient, PrivacyPolicy privacyPolicy,
        EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    private readonly PrivacyPolicy _privacyPolicy = privacyPolicy;

    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        RecursivePixivAsyncEnumerators.Illustration<RecentPostedIllustrationEngine>
            .WithInitialUrl(this, MakoApiKind.AppApi, engine => "/v2/illust/follow"
                                                                + $"?restrict={engine._privacyPolicy.GetDescription()}")!;
}
