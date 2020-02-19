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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class RecommendIllustratorDeferrer
    {
        public static readonly RecommendIllustratorDeferrer Instance = new RecommendIllustratorDeferrer();

        private readonly List<User> currentIllustrators = new List<User>();

        private int index;

        private int requestTimes;

        public async Task<IEnumerable<User>> Acquire(int count)
        {
            Contract.Requires<ArgumentException>(30 % count == 0, "count must be divisible by 30");
            if (currentIllustrators.Count < index + count)
            {
                if (requestTimes >= 9)
                {
                    index = 0;
                    requestTimes = 0;
                    currentIllustrators.Clear();
                }

                await Request();
            }

            var illustrators = currentIllustrators.Skip(index).Take(count);
            index += count;
            return illustrators;
        }

        private async Task Request()
        {
            var newIllustrators = await HttpClientFactory.AppApiService.GetRecommendIllustrators(new RecommendIllustratorRequest {Offset = requestTimes++ * 30});
            currentIllustrators.AddRange(newIllustrators.UserPreviews.Select(i => i.Parse()));
        }
    }
}