#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/UserSearchEngine.cs
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
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements
{
    public class UserSearchEngine : AbstractPixivFetchEngine<User>
    {
        private readonly string _keyword;
        private readonly TargetFilter _targetFilter;
        private readonly UserSortOption _userSortOption;

        public UserSearchEngine([NotNull] MakoClient makoClient, TargetFilter targetFilter, UserSortOption? userSortOption, string keyword, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _keyword = keyword;
            _targetFilter = targetFilter;
            _userSortOption = userSortOption ?? UserSortOption.DateDescending;
        }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            var sortSegment = _userSortOption != UserSortOption.DoNotSort ? $"&sort={_userSortOption.GetDescription()}" : string.Empty;
            return RecursivePixivAsyncEnumerators.User<UserSearchEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => $"https://app-api.pixiv.net/v1/search/user?filter={engine._targetFilter.GetDescription()}&word={engine._keyword}{sortSegment}")!;
        }
    }
}