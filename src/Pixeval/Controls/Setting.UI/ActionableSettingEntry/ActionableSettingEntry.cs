#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ActionableSettingEntry.cs
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

namespace Pixeval.Controls.Setting.UI.ActionableSettingEntry
{
    [ContentProperty(Name = nameof(ActionContent))]
    public class ActionableSettingEntry : SettingEntryBase
    {
        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(ActionableSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public ActionableSettingEntry()
        {
            DefaultStyleKey = typeof(ActionableSettingEntry);
        }

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }
    }
}