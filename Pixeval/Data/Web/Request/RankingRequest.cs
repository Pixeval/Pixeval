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

using Refit;

namespace Pixeval.Data.Web.Request
{
    public class RankingRequest
    {
        [AliasAs("content_type")]
        public string ContentType { get; set; } = "illust";

        [AliasAs("filter")]
        public string Filter { get; set; } = "for_ios";

        [AliasAs("min_bookmark_id_for_recent_illust")]
        public string MinBookmarkId { get; set; } = null;

        [AliasAs("max_bookmark_id_for_recommend")]
        public string MaxBookmarkId { get; set; } = null;
    }
}