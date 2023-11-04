#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/GlyphAttribute.cs
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
using System.Reflection;

namespace Pixeval.Controls.MarkupExtensions.FontSymbolIcon;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class GlyphAttribute(char glyph) : Attribute
{
    public char Glyph { get; } = glyph;
}

public static class GlyphAttributeHelper
{
    public static char GetGlyph(this Enum e)
    {
        return e.GetType().GetField(e.ToString())!.GetCustomAttribute<GlyphAttribute>()!.Glyph;
    }
}
