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

using System;
using System.Linq;
using Pixeval.Wpf.Core;
using Pixeval.Wpf.Objects.Generic;
using Pixeval.Wpf.Objects.I18n;
using Pixeval.Wpf.Objects.Primitive;
using PropertyChanged;

namespace Pixeval.Wpf.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class RankOptionModel
    {
        [DoNotNotify]
        public static readonly RankOptionModel[] RegularRankOptions = Enum.GetValues(typeof(RankOption))
            .Cast<RankOption>().Select(rank => new RankOptionModel(rank)).ToArray()
            .Apply(models => models[0].IsSelected = true);

        [DoNotNotify]
        public static readonly DateTime MaxRankDateTime = DateTime.Today - TimeSpan.FromDays(2);

        [DoNotNotify]
        public static readonly DateTime InvalidRankDateTimeStart = DateTime.Today - TimeSpan.FromDays(1);

        public RankOptionModel(RankOption option)
        {
            Corresponding = option;
            Name = AkaI18N.GetResource(option.GetEnumAttribute<EnumLocalizedName>().Name);
        }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public RankOption Corresponding { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
