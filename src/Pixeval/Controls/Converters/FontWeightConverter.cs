using System;
using Microsoft.UI.Xaml.Data;
using Windows.UI.Text;
using Microsoft.UI.Text;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;
internal class FontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value.To<FontWeightsOption>() switch
    {
        FontWeightsOption.Thin => FontWeights.Thin,
        FontWeightsOption.ExtraLight => FontWeights.ExtraLight,
        FontWeightsOption.Light => FontWeights.Light,
        FontWeightsOption.SemiLight => FontWeights.SemiLight,
        FontWeightsOption.Normal => FontWeights.Normal,
        FontWeightsOption.Medium => FontWeights.Medium,
        FontWeightsOption.SemiBold => FontWeights.SemiBold,
        FontWeightsOption.Bold => FontWeights.Bold,
        FontWeightsOption.ExtraBold => FontWeights.ExtraBold,
        FontWeightsOption.Black => FontWeights.Black,
        FontWeightsOption.ExtraBlack => FontWeights.ExtraBlack,
        _ => ThrowHelper.ArgumentOutOfRange<object, FontWeight>(value)
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language) => ThrowHelper.NotSupported<object>();
}
