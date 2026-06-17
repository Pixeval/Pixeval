// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;

namespace Pixeval.Controls;

/// <summary>
/// Dependency properties for the <see cref="ConstrainedBox"/> class.
/// </summary>
public partial class ConstrainedBox
{
    /// <summary>
    /// Gets or sets the scale for the width of the panel. Should be a value between 0-1.0. Default is 1.0.
    /// </summary>
    public double ScaleX
    {
        get;
        set
        {
            value = value switch
            {
                < 0 => 0,
                double.NaN or > 1.0 => 1.0,
                _ => value
            };

            SetAndRaise(ScaleXProperty, ref field, value);
        }
    } = 1;

    /// <summary>
    /// Identifies the <see cref="ScaleX"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, double> ScaleXProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, double>(nameof(ScaleX), t => t.ScaleX, (t, v) => t.ScaleX = v, 1);

    /// <summary>
    /// Gets or sets the scale for the height of the panel. Should be a value between 0-1.0. Default is 1.0.
    /// </summary>
    public double ScaleY
    {
        get;
        set
        {
            value = value switch
            {
                < 0 => 0,
                double.NaN or > 1.0 => 1.0,
                _ => value
            };

            SetAndRaise(ScaleYProperty, ref field, value);
        }
    } = 1;

    /// <summary>
    /// Identifies the <see cref="ScaleY"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, double> ScaleYProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, double>(nameof(ScaleY), t => t.ScaleY, (t, v) => t.ScaleY = v, 1);

    /// <summary>
    /// Gets or sets the integer multiple that the width of the panel should be floored to. Default is null (no snap).
    /// </summary>
    public int MultipleX
    {
        get;
        set
        {
            value = int.Max(-1, value);
            SetAndRaise(MultipleXProperty, ref field, value);
        }
    } = -1;

    /// <summary>
    /// Identifies the <see cref="MultipleX"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, int> MultipleXProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, int>(nameof(MultipleX), t => t.MultipleX, (t, v) => t.MultipleX = v, -1);

    /// <summary>
    /// Gets or sets the integer multiple that the height of the panel should be floored to. Default is null (no snap).
    /// </summary>
    public int MultipleY
    {
        get;
        set
        {
            value = int.Max(-1, value);
            SetAndRaise(MultipleYProperty, ref field, value);
        }
    } = -1;

    /// <summary>
    /// Identifies the <see cref="MultipleY"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, int> MultipleYProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, int>(nameof(MultipleY), t => t.MultipleX, (t, v) => t.MultipleX = v, -1);

    /// <summary>
    /// Gets or sets aspect Ratio to use for the contents of the Panel (after scaling).
    /// </summary>
    public AspectRatio AspectRatio
    {
        get;
        set
        {
            if (value.Width <= 0 || value.Height <= 0)
                value = AspectRatio.NullValue;
            SetAndRaise(AspectRatioProperty, ref field, value);
        }
    } = AspectRatio.NullValue;

    /// <summary>
    /// Identifies the <see cref="AspectRatio"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, AspectRatio> AspectRatioProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, AspectRatio>(nameof(AspectRatio), t => t.AspectRatio, (t, v) => t.AspectRatio = v, AspectRatio.NullValue);

    /// <summary>
    /// Gets or sets a value indicating whether the panel should be restricted to the specified aspect ratio during the arrangement phase. If false, the panel will only be restricted during the measure phase. Default is true.
    /// </summary>
    public bool RestrictArrange
    {
        get;
        set => SetAndRaise(RestrictArrangeProperty, ref field, value);
    } = true;

    /// <summary>
    /// Identifies the <see cref="RestrictArrange"/> property.
    /// </summary>
    public static readonly DirectProperty<ConstrainedBox, bool> RestrictArrangeProperty =
        AvaloniaProperty.RegisterDirect<ConstrainedBox, bool>(nameof(RestrictArrange), t => t.RestrictArrange, (t, v) => t.RestrictArrange = v, true);
}
