#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/CultureDateConverter.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.UI.Xaml.Data;
using Pixeval.AppManagement;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class CultureDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTime t => t.ToString(AppSettings.CurrentCulture.DateTimeFormat.ShortDatePattern),
            DateTimeOffset t => t.ToString(AppSettings.CurrentCulture.DateTimeFormat.ShortDatePattern),
            _ => ThrowHelper.ArgumentOutOfRange<object, string>(value)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => ThrowHelper.NotSupported<object>();
}
