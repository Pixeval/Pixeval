#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/NumberToPercentageConverter.cs
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
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class NumberToPercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var p = "F" + parameter.To<string>();
        return value switch
        {
            uint i => (i * 100).ToString(p),
            int i => (i * 100).ToString(p),
            short i => (i * 100).ToString(p),
            ushort i => (i * 100).ToString(p),
            long i => (i * 100).ToString(p),
            ulong i => (i * 100).ToString(p),
            float i => (i * 100).ToString(p),
            double i => (i * 100).ToString(p),
            decimal i => (i * 100).ToString(p),
            _ => "NaN"
        } + "%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
