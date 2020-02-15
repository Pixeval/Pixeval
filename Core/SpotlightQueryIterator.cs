// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;

namespace Pixeval.Core
{
    public class SpotlightQueryIterator : IPixivIterator<SpotlightArticle>
    {
        private readonly int endPoint;

        private int currentIndex;

        public SpotlightQueryIterator(int start, int queryPages)
        {
            currentIndex = start < 1 ? 1 : start;
            endPoint = currentIndex + queryPages - 1;
        }

        public SortOption SortOption { get; } = SortOption.None;

        public bool HasNext()
        {
            return currentIndex <= endPoint;
        }

        public async IAsyncEnumerable<SpotlightArticle> MoveNextAsync()
        {
            var spotlight = await HttpClientFactory.AppApiService.GetSpotlights(currentIndex++ * 10);
            foreach (var spotlightSpotlightArticle in spotlight.SpotlightArticles) yield return spotlightSpotlightArticle;
        }
    }
}