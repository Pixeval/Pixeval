#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RankingEngine.cs
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

using System;
using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class RankingEngine : AbstractPixivFetchEngine<Illustration>
{
    private readonly DateTime _dateTime;
    private readonly RankOption _rankOption;
    private readonly TargetFilter _targetFilter;

    public RankingEngine(
        MakoClient makoClient,
        RankOption rankOption,
        DateTime dateTime,
        TargetFilter targetFilter,
        EngineHandle? engineHandle) : base(makoClient, engineHandle)
    {
        _rankOption = rankOption;
        _dateTime = dateTime;
        _targetFilter = targetFilter;
    }

    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return RecursivePixivAsyncEnumerators.Illustration<RankingEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
            engine => "/v1/illust/ranking"
                      + $"?filter={engine._targetFilter.GetDescription()}"
                      + $"&mode={engine._rankOption.GetDescription()}"
                      + $"&date={engine._dateTime:yyyy-MM-dd}")!;
    }
}