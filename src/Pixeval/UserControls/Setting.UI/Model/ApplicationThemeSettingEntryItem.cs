#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ApplicationThemeSettingEntryItem.cs
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
using System.Collections.Generic;
using System.Linq;
using Pixeval.Options;

namespace Pixeval.UserControls.Setting.UI.Model;

public record ApplicationThemeSettingEntryItem : StringRepresentableItem, IAvailableItems
{
    public ApplicationThemeSettingEntryItem(ApplicationTheme item) : base(item)
    {
    }

    public static IEnumerable<StringRepresentableItem> AvailableItems { get; } = Enum.GetValues<ApplicationTheme>().Select(a => new ApplicationThemeSettingEntryItem(a));
}