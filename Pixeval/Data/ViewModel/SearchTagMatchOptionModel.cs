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

using System.Collections.Generic;
using Pixeval.Core;
using Pixeval.Core.Options;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class SearchTagMatchOptionModel
    {
        public static readonly SearchTagMatchOptionModel PartialMatchModel = new SearchTagMatchOptionModel(SearchTagMatchOption.PartialMatchForTags);

        public static readonly SearchTagMatchOptionModel ExactMatchModel = new SearchTagMatchOptionModel(SearchTagMatchOption.ExactMatchForTags);

        public static readonly SearchTagMatchOptionModel TitleAndCaptionModel = new SearchTagMatchOptionModel(SearchTagMatchOption.TitleAndCaption);

        public static readonly IEnumerable<SearchTagMatchOptionModel> AllPossibleMatchOptions = new[] { PartialMatchModel, ExactMatchModel, TitleAndCaptionModel };

        public SearchTagMatchOptionModel(SearchTagMatchOption corresponding)
        {
            Description = AkaI18N.GetResource(corresponding.GetEnumAttribute<EnumLocalizedName>().Name);
            Corresponding = corresponding;
        }

        public string Description { get; set; }

        public SearchTagMatchOption Corresponding { get; set; }
    }
}