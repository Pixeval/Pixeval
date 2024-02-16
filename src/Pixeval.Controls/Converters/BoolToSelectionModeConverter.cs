using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class BoolToSelectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value.To<bool>() ? ItemsViewSelectionMode.Multiple : ItemsViewSelectionMode.None;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value.To<ItemsViewSelectionMode>() is ItemsViewSelectionMode.Multiple;
}
