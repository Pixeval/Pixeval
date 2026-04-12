// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Avalonia;
using Avalonia.AnimatedImage;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Controls;

public static class Source
{
    public static readonly AttachedProperty<string?> AnimatedBitmapProperty =
        AvaloniaProperty.RegisterAttached<AnimatedImage, string?>(
            "AnimatedBitmap",
            typeof(Source),
            defaultValue: null);

    static Source()
    {
        AnimatedBitmapProperty.Changed.AddClassHandler<AnimatedImage>(OnAnimatedBitmapChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetAnimatedBitmap(AnimatedImage element) => element.GetValue(AnimatedBitmapProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetAnimatedBitmap(AnimatedImage element, string? value) => element.SetValue(AnimatedBitmapProperty, value);

    private static async void OnAnimatedBitmapChanged(AnimatedImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value)
        {
            element.Source = null;
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapFromCacheAsync(value);

        if (element.GetValue(AnimatedBitmapProperty) == value)
            element.Source = bitmap;
    }
}
