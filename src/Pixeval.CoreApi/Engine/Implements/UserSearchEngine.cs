#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/UserSearchEngine.cs
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

public class UserSearchEngine(MakoClient makoClient, TargetFilter targetFilter, UserSortOption? userSortOption,
        string keyword, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<User>(makoClient, engineHandle)
{
    private readonly string _keyword = keyword;
    private readonly TargetFilter _targetFilter = targetFilter;
    private readonly UserSortOption _userSortOption = userSortOption ?? UserSortOption.DateDescending;

    public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        var sortSegment = _userSortOption != UserSortOption.DoNotSort ? $"&sort={_userSortOption.GetDescription()}" : string.Empty;
        return RecursivePixivAsyncEnumerators.User<UserSearchEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
            engine => $"https://app-api.pixiv.net/v1/search/user?filter={engine._targetFilter.GetDescription()}&word={engine._keyword}{sortSegment}")!;
    }
}
