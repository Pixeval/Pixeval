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
using System.Globalization;
using System.Windows.Data;
using Pixeval.Wpf.Objects.I18n;
using Pixeval.Wpf.ViewModel;

namespace Pixeval.Wpf.Objects.ValueConverters
{
    [ValueConversion(typeof(TrendType), typeof(string))]
    public class TrendsStatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TrendType obj)
                return obj switch
                {
                    TrendType.AddIllust => AkaI18N.TrendsAddIllust,
                    TrendType.AddBookmark => AkaI18N.TrendsAddBookmark,
                    TrendType.AddFavorite => AkaI18N.TrendsAddFavorite,
                    _ => string.Empty
                };

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
