// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using Windows.UI.Text;
using Microsoft.UI.Text;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// Converters
/// </summary>
public static class C
{
    public static bool Negation(bool value) => !value;

    public static bool IsNull(object? value) => value is null;

    public static bool IsNotNull(object? value) => value is not null;

    public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToVisibilityNegation(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility IsNullToVisibility(object? value) => value is null ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility IsNotNullToVisibility(object? value) => value is null ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility IsEqualToVisibility(object? x, object? y) => Equals(x, y) ? Visibility.Visible : Visibility.Collapsed;
    
    public static Visibility IsNotEqualToVisibility(object? x, object? y) => Equals(x, y) ? Visibility.Collapsed : Visibility.Visible;

    public static bool IsZeroD(double value) => value < double.Epsilon;

    public static bool IsNotZero(int value) => value is not 0;

    public static bool IsNotZeroL(long value) => value is not 0;

    public static Visibility IsNotZeroToVisibility(int value) => value is not 0 ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility IsNotZeroDToVisibility(double value) => value is not 0 ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility IsNullOrEmptyToVisibility(string? value) => value.IsNullOrEmpty() ? Visibility.Collapsed : Visibility.Visible;

    public static unsafe Color ToAlphaColor(uint color)
    {
        var ptr = &color;
        var c = (byte*)ptr;
        return Color.FromArgb(c[3], c[2], c[1], c[0]);
    }

    public static SolidColorBrush ToSolidColorBrush(uint value) => new(ToAlphaColor(value));

    public static unsafe uint ToAlphaUInt(Color color)
    {
        uint ret;
        var ptr = &ret;
        var c = (byte*)ptr;
        c[0] = color.B;
        c[1] = color.G;
        c[2] = color.R;
        c[3] = color.A;
        return ret;
    }

    public static string CultureDateTimeDateFormatter(DateTime value, CultureInfo culture) =>
        value.ToString(culture.DateTimeFormat.ShortDatePattern);

    public static string CultureDateTimeOffsetDateFormatter(DateTimeOffset value, CultureInfo culture) =>
        value.ToString(culture.DateTimeFormat.ShortDatePattern);

    public static string CultureDateTimeFormatter(DateTime value, CultureInfo culture) =>
        value.ToString(culture.DateTimeFormat.FullDateTimePattern);

    public static string CultureDateTimeOffsetFormatter(DateTimeOffset value, CultureInfo culture) =>
        value.ToString(culture.DateTimeFormat.FullDateTimePattern);

    public static FontFamily ToFontFamily(string value) => new(value);

    public static object? FirstOrDefault(object value) => value is IEnumerable e ? e.OfType<object>().FirstOrDefault() : null;

    public static string ToPercentageString(object value, int precision)
    {
        var p = "F" + precision;
        return value switch
        {
            uint i => (i * 100).ToString(p),
            int i => (i * 100).ToString(p),
            short i => (i * 100).ToString(p),
            ushort i => (i * 100).ToString(p),
            long i => (i * 100).ToString(p),
            ulong i => (i * 100).ToString(p),
            float i => (i * 100).ToString(p),
            double i => (i * 100).ToString(p),
            decimal i => (i * 100).ToString(p),
            _ => "NaN"
        } + "%";
    }

    public static string PlusOneToString(int value) => (value + 1).ToString();

    public static CommandBarLabelPosition LabelIsNullToVisibility(string? value) =>
        value is null ? CommandBarLabelPosition.Collapsed : CommandBarLabelPosition.Default;

    public static ItemsViewSelectionMode ToSelectionMode(bool value) =>
        value ? ItemsViewSelectionMode.Multiple : ItemsViewSelectionMode.None;

    public static string IntEllipsis(int value) =>
        value < 1000 ? value.ToString() : $"{value / 1000d:0.#}k";

    public static double DoubleComplementary(double value) => 1 - value;

    public static FontWeight ToFontWeight(object value) => (value as Enum)?.GetHashCode() switch
    {
        0 => FontWeights.Thin,
        1 => FontWeights.ExtraLight,
        2 => FontWeights.Light,
        3 => FontWeights.SemiLight,
        4 => FontWeights.Normal,
        5 => FontWeights.Medium,
        6 => FontWeights.SemiBold,
        7 => FontWeights.Bold,
        8 => FontWeights.ExtraBold,
        9 => FontWeights.Black,
        10 => FontWeights.ExtraBlack,
        _ => ThrowHelper.ArgumentOutOfRange<object, FontWeight>(value)
    };
}
