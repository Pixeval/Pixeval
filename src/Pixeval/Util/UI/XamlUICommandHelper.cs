#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/XamlUICommandHelper.cs
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

using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pixeval.Util.UI;

public static class XamlUICommandHelper
{
    public static XamlUICommand GetCommand(this string label, FontIconSymbols icon)
    {
        return new()
        {
            Label = label,
            Description = label,
            IconSource = icon.GetFontIconSource()
        };
    }

    public static XamlUICommand GetCommand(this string label, FontIconSymbols icon, VirtualKey key)
    {
        return new()
        {
            Label = label,
            IconSource = icon.GetFontIconSource(),
            KeyboardAccelerators = { new KeyboardAccelerator { Key = key } }
        };
    }

    public static XamlUICommand GetCommand(this string label, FontIconSymbols icon, VirtualKeyModifiers modifiers, VirtualKey key)
    {
        return new()
        {
            Label = label,
            IconSource = icon.GetFontIconSource(),
            KeyboardAccelerators = { new KeyboardAccelerator { Modifiers = modifiers, Key = key } }
        };
    }

    public static XamlUICommand GetCommand(this string label, IconSource icon, VirtualKeyModifiers modifiers, VirtualKey key)
    {
        return new()
        {
            Label = label,
            IconSource = icon,
            KeyboardAccelerators = { new KeyboardAccelerator { Modifiers = modifiers, Key = key } }
        };
    }
}
