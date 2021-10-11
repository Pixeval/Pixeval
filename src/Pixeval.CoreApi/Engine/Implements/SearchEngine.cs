#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/SearchEngine.cs
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

using System;
using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements
{
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
}