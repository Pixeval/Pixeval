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