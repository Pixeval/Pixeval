using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class BoolToCommandBarLabelPositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value.To<bool>() ? CommandBarLabelPosition.Default : CommandBarLabelPosition.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value.To<CommandBarLabelPosition>() is CommandBarLabelPosition.Default;
}
