#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/SearchEngine.cs
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

internal class SearchEngine : AbstractPixivFetchEngine<Illustration>
{
    private readonly int _current;
    private readonly DateTimeOffset? _endDate;
    private readonly SearchTagMatchOption _matchOption;
    private readonly int _pages;
    private readonly SearchDuration _searchDuration;
    private readonly IllustrationSortOption _sortOption;
    private readonly DateTimeOffset? _startDate;
    private readonly string _tag;
    private readonly TargetFilter _targetFilter;

    public SearchEngine(
        MakoClient makoClient,
        EngineHandle? engineHandle,
        SearchTagMatchOption matchOption,
        string tag,
        int start,
        int pages,
        IllustrationSortOption? sortOption,
        SearchDuration searchDuration,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        TargetFilter? targetFilter) : base(makoClient, engineHandle)
    {
        _matchOption = matchOption;
        _tag = tag;
        _current = start;
        _pages = pages;
        _sortOption = sortOption ?? IllustrationSortOption.PublishDateDescending;
        _searchDuration = searchDuration;
        _startDate = startDate;
        _endDate = endDate;
        _targetFilter = targetFilter ?? TargetFilter.ForAndroid;
    }

    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new SearchAsyncEnumerator(this, MakoApiKind.AppApi)!;
    }

    private class SearchAsyncEnumerator : RecursivePixivAsyncEnumerators.Illustration<SearchEngine>
    {
        public SearchAsyncEnumerator(SearchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
        {
        }

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