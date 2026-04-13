// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Avalonia;
using Avalonia.AnimatedImage;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Controls;

public static class Source
{
    public static readonly AttachedProperty<string?> AnimatedImageProperty =
        AvaloniaProperty.RegisterAttached<AnimatedImage, string?>(
            "AnimatedImage",
            typeof(Source),
            defaultValue: null);

    public static readonly AttachedProperty<string?> AvatarImageProperty =
        AvaloniaProperty.RegisterAttached<AvatarImage, string?>(
            "AvatarImage",
            typeof(Source),
            defaultValue: null);

    static Source()
    {
        AnimatedImageProperty.Changed.AddClassHandler<AnimatedImage>(OnAnimatedImageChanged);
        AvatarImageProperty.Changed.AddClassHandler<AvatarImage>(OnAvatarImageChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetAnimatedImage(AnimatedImage element) => element.GetValue(AnimatedImageProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetAvatarImage(AvatarImage element) => element.GetValue(AvatarImageProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetAnimatedImage(AnimatedImage element, string? value) => element.SetValue(AnimatedImageProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetAvatarImage(AvatarImage element, string? value) => element.SetValue(AvatarImageProperty, value);

    private static async void OnAnimatedImageChanged(AnimatedImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value)
        {
            element.Source = null;
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapFromCacheAsync(value);

        if (element.GetValue(AnimatedImageProperty) == value)
            element.Source = bitmap;
    }

    private static async void OnAvatarImageChanged(AvatarImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value)
        {
            element.Source = null;
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapFromCacheAsync(value);

        if (element.GetValue(AvatarImageProperty) == value)
            element.Source = bitmap;
    }
}
