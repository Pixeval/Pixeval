#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/ColorHelper.cs
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

using Windows.UI;

namespace Pixeval.Controls;

public static class ColorHelper
{
    public static unsafe Color GetAlphaColor(this uint color)
    {
        var ptr = &color;
        var c = (byte*)ptr;
        return Color.FromArgb(c[3], c[2], c[1], c[0]);
    }

    public static unsafe uint GetAlphaUInt(this Color color)
    {
        uint ret;
        var ptr = &ret;
        var c = (byte*)ptr;
        c[0] = color.B;
        c[1] = color.G;
        c[2] = color.R;
        c[3] = color.A;
        return ret;
    }
}
