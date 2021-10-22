#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ExpanderSettingEntry.cs
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
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.ExpanderSettingEntry
{
    [ContentProperty(Name = nameof(Content))]
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    public class ExpanderSettingEntry : SettingEntryBase
    {
        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            nameof(ContentMargin),
            typeof(Thickness),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public ExpanderSettingEntry()
        {
            DefaultStyleKey = typeof(ExpanderSettingEntry);
        }

        public double HeaderHeight
        {
            get => (double) GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public Thickness ContentMargin
        {
            get => (Thickness) GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }
    }
}