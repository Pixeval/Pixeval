using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class SearchEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly SearchTagMatchOption _matchOption;
        private readonly IllustrationSortOption _sortOption;
        private readonly SearchDuration? _searchDuration;
        private readonly TargetFilter _targetFilter;
        private readonly DateTime? _startDate;
        private readonly DateTime? _endDate;
        private readonly string _tag;
        private readonly int _pages;
        private readonly int _current;

        /// <summary>
        /// 实例化一个<see cref="SearchEngine"/>
        /// </summary>
        /// <param name="makoClient">与该<see cref="SearchEngine"/>绑定的<see cref="MakoClient"/></param>
        /// <param name="engineHandle">该<see cref="SearchEngine"/>的句柄</param>
        /// <param name="matchOption">关键字匹配选项</param>
        /// <param name="tag">关键字</param>
        /// <param name="start">从第几页开始搜索(每页包含30条结果)</param>
        /// <param name="pages">搜索页数</param>
        /// <param name="sortOption">如何排序结果</param>
        /// <param name="searchDuration">搜索时间区间</param>
        /// <param name="startDate">日期区间开始</param>
        /// <param name="endDate">日期区间结束</param>
        /// <param name="targetFilter"></param>
        public SearchEngine(
            MakoClient makoClient, 
            EngineHandle? engineHandle,
            SearchTagMatchOption matchOption,
            string tag,
            int start,
            int pages,
            IllustrationSortOption? sortOption, 
            SearchDuration? searchDuration, 
            DateTime? startDate,
            DateTime? endDate, 
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
            public SearchAsyncEnumerator([NotNull] SearchEngine pixivFetchEngine, MakoApiKind makoApiKind) : base(pixivFetchEngine, makoApiKind)
            {
            }
            
            protected override string InitialUrl() => GetSearchUrl();

            protected override bool HasNextPage()
            {
                return PixivFetchEngine.RequestedPages <= PixivFetchEngine._pages - 1;
            }

            private string GetSearchUrl()
            {
                var match = PixivFetchEngine._matchOption.GetDescription();
                var startDateSegment = PixivFetchEngine._startDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}");
                var endDateSegment = PixivFetchEngine._endDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}");
                var durationSegment = PixivFetchEngine._searchDuration?.Let(du => $"&duration={du.GetDescription()}");
                return $"/v1/search/illust?search_target={match}&word={PixivFetchEngine._tag}&filter={PixivFetchEngine._targetFilter.GetDescription()}&offset={PixivFetchEngine._current}&sort={PixivFetchEngine._sortOption.GetDescription()}{startDateSegment}{endDateSegment}{durationSegment}";
            }
        }
    }
}