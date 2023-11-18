#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/SearchEngine.cs
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

internal class SearchEngine(MakoClient makoClient,
        EngineHandle? engineHandle,
        SearchTagMatchOption matchOption,
        string tag,
        int start,
        int pages,
        IllustrationSortOption? sortOption,
        SearchDuration searchDuration,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        TargetFilter? targetFilter)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    private readonly int _current = start;
    private readonly DateTimeOffset? _endDate = endDate;
    private readonly SearchTagMatchOption _matchOption = matchOption;
    private readonly int _pages = pages;
    private readonly SearchDuration _searchDuration = searchDuration;
    private readonly IllustrationSortOption _sortOption = sortOption ?? IllustrationSortOption.PublishDateDescending;
    private readonly DateTimeOffset? _startDate = startDate;
    private readonly string _tag = tag;
    private readonly TargetFilter _targetFilter = targetFilter ?? TargetFilter.ForAndroid;

    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new SearchAsyncEnumerator(this, MakoApiKind.AppApi)!;
    }

    private class SearchAsyncEnumerator
        (SearchEngine pixivFetchEngine, MakoApiKind makoApiKind) : RecursivePixivAsyncEnumerators.Illustration<SearchEngine>(pixivFetchEngine, makoApiKind)
    {
        protected override string InitialUrl()
        {
            return GetSearchUrl();
        }

        protected override bool HasNextPage()
        {
            return PixivFetchEngine.RequestedPages <= PixivFetchEngine._pages - 1;
        }

        private string GetSearchUrl()
        {
            var match = PixivFetchEngine._matchOption.GetDescription();
            var startDateSegment = PixivFetchEngine._startDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}");
            var endDateSegment = PixivFetchEngine._endDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}");
            var durationSegment = PixivFetchEngine._searchDuration == SearchDuration.Undecided ? null : $"&duration={PixivFetchEngine._searchDuration.GetDescription()}";
            var sortSegment = PixivFetchEngine._sortOption != IllustrationSortOption.DoNotSort ? $"&sort={PixivFetchEngine._sortOption.GetDescription()}" : string.Empty;
            return "/v1/search/illust"
                   + $"?search_target={match}"
                   + $"&word={PixivFetchEngine._tag}"
                   + $"&filter={PixivFetchEngine._targetFilter.GetDescription()}"
                   + $"&offset={PixivFetchEngine._current}"
                   + $"{sortSegment}{startDateSegment}{endDateSegment}{durationSegment}";
        }
    }
}