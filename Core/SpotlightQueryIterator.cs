// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Collections.Generic;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;

namespace Pixeval.Core
{
    public class SpotlightQueryIterator
    {
        private readonly int queryPages;
        private readonly int start;

        public SpotlightQueryIterator(int start, int queryPages)
        {
            this.start = start < 1 ? 1 : start;
            this.queryPages = queryPages;
        }

        public async IAsyncEnumerable<SpotlightArticle> GetSpotlight()
        {
            for (var i = start; i < start + queryPages - 1; i++)
            {
                var spotlight = await HttpClientFactory.AppApiService.GetSpotlights(i * 10);
                foreach (var spotlightSpotlightArticle in spotlight.SpotlightArticles) yield return spotlightSpotlightArticle;
            }
        }
    }
}