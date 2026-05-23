// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Pixeval.Controls;

/// <summary>
/// Converter for converting between <see cref="AspectRatio"/> and string.
/// </summary>
public class AspectRatioConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
            return AspectRatio.ConvertToAspectRatio(str);
        throw new NotSupportedException($"Cannot convert {value?.GetType().Name ?? "null"} to {nameof(AspectRatio)}");
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is AspectRatio aspect && destinationType == typeof(string))
            return aspect.ToString();
        throw new NotSupportedException($"Cannot convert {nameof(AspectRatio)} to {destinationType.Name}");
    }
}

/// <summary>
/// The <see cref="AspectRatio"/> structure is used by the <see cref="ConstrainedBox"/> control to
/// define a specific ratio to restrict its content.
/// </summary>
[TypeConverter(typeof(AspectRatioConverter))]
public readonly record struct AspectRatio
{
    public static readonly AspectRatio NullValue = new AspectRatio(0, 0);

    /// <summary>
    /// Gets the width component of the aspect ratio or the aspect ratio itself (and height will be 1).
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the height component of the aspect ratio.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Gets the raw numerical aspect ratio value itself (Width / Height).
    /// </summary>
    public double Value => Width / Height;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspectRatio"/> struct with the provided width and height.
    /// </summary>
    /// <param name="width">Width side of the ratio.</param>
    /// <param name="height">Height side of the ratio.</param>
    public AspectRatio(double width, double height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AspectRatio"/> struct with the specific numerical aspect ratio.
    /// </summary>
    /// <param name="ratio">Raw Aspect Ratio, Height will be 1.</param>
    public AspectRatio(double ratio)
    {
        Width = ratio;
        Height = 1;
    }

    /// <summary>
    /// Implicit conversion operator to convert an <see cref="AspectRatio"/> to a <see cref="double"/> value.
    /// This lets you use them easily in mathematical expressions.
    /// </summary>
    /// <param name="aspect"><see cref="AspectRatio"/> instance.</param>
    public static implicit operator double(AspectRatio aspect) => aspect.Value;

    /// <summary>
    /// Implicit conversion operator to convert a <see cref="double"/> to an <see cref="AspectRatio"/> value.
    /// This allows for x:Bind to bind to a double value.
    /// </summary>
    /// <param name="ratio"><see cref="double"/> value representing the <see cref="AspectRatio"/>.</param>
    public static implicit operator AspectRatio(double ratio) => new AspectRatio(ratio);

    /// <summary>
    /// Implicit conversion operator to convert a <see cref="int"/> to an <see cref="AspectRatio"/> value.
    /// Creates a simple aspect ratio of N:1, where N is int
    /// </summary>
    /// <param name="width"><see cref="int"/> value representing the <see cref="AspectRatio"/>.</param>
    public static implicit operator AspectRatio(int width) => new AspectRatio(width, 1.0);

    /// <summary>
    /// Converter to take a string aspect ratio like "16:9" and convert it to an <see cref="AspectRatio"/> struct.
    /// Used automatically by XAML.
    /// </summary>
    /// <param name="rawString">The string to be converted in format "Width:Height" or a decimal value.</param>
    /// <returns>The <see cref="AspectRatio"/> struct representing that ratio.</returns>
    public static AspectRatio ConvertToAspectRatio(string rawString)
    {
        var ratio = rawString.Split(':');

        switch (ratio)
        {
            case [var ratio0, var ratio1]:
            {
                var width = double.Parse(ratio0, NumberStyles.Float, CultureInfo.InvariantCulture);
                var height = double.Parse(ratio1, NumberStyles.Float, CultureInfo.InvariantCulture);

                return new AspectRatio(width, height);
            }
            case [var ratio0]:
                return new AspectRatio(double.Parse(ratio0, NumberStyles.Float, CultureInfo.InvariantCulture));
            default:
                return new AspectRatio(1);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Width + ":" + Height;
    }
}
