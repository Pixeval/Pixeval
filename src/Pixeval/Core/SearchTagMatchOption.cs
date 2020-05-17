#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using Pixeval.Objects.Primitive;

namespace Pixeval.Core
{
    public enum SearchTagMatchOption
    {
        /// <summary>
        ///     部分一致
        /// </summary>
        [EnumAlias("partial_match_for_tags")] [EnumLocalizedName("TagMatchingPartialMatch")]
        PartialMatchForTags,

        /// <summary>
        ///     完全一致
        /// </summary>
        [EnumAlias("exact_match_for_tags")] [EnumLocalizedName("TagMatchingExactMatch")]
        ExactMatchForTags,

        /// <summary>
        ///     标题和说明文
        /// </summary>
        [EnumAlias("title_and_caption")] [EnumLocalizedName("TagMatchingTitleAndCaption")]
        TitleAndCaption
    }
}