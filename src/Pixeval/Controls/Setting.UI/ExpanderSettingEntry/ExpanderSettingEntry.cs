#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ExpanderSettingEntry.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using WinUI3Utilities.Attributes;
using Pixeval.Controls.Setting.UI.UserControls;
using Expander = Pixeval.Controls.Expander.Expander;


namespace Pixeval.Controls.Setting.UI.ExpanderSettingEntry;

/// <summary>
/// <see cref="ExpanderSettingEntry"/> is a setting entry with an <see cref="Expander"/>
/// </summary>
[ContentProperty(Name = "Content")]
[TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
[DependencyProperty<double>("HeaderHeight")]
[DependencyProperty<object>("Content")] // The content of the Expander
[DependencyProperty<Thickness>("ContentMargin")]
public partial class ExpanderSettingEntry : SettingEntryBase
{
    public ExpanderSettingEntry()
    {
        DefaultStyleKey = typeof(ExpanderSettingEntry);
    }
}
