#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AdaptiveHeightValueConverter.cs
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
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Pixeval.CommunityToolkit.AdaptiveGridView
{
    internal class AdaptiveHeightValueConverter : IValueConverter
    {
        public Thickness DefaultItemMargin { get; set; } = new(0, 0, 4, 4);

        public object Convert(object? value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var gridView = parameter as GridView;
                if (gridView == null)
                {
                    return value;
                }

                double.TryParse(value.ToString(), out var height);

                var padding = gridView.Padding;
                var margin = GetItemMargin(gridView, DefaultItemMargin);
                height = height + margin.Top + margin.Bottom + padding.Top + padding.Bottom;

                return height;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        internal static Thickness GetItemMargin(GridView view, Thickness fallback = default)
        {
            var setter = view.ItemContainerStyle?.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == FrameworkElement.MarginProperty);
            if (setter != null)
            {
                return (Thickness) setter.Value;
            }

            if (view.Items.Count > 0 && view.ContainerFromIndex(0) is GridViewItem container)
            {
                return container.Margin;
            }

            // Use the default thickness for a GridViewItem
            return fallback;
        }
    }
}