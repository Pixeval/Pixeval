#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/UIntToBrushConverter.cs
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
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class UIntToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => new SolidColorBrush(value.To<uint>().GetAlphaColor());

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value.To<SolidColorBrush>().Color.GetAlphaUInt();
}
