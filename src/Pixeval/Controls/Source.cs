// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using Avalonia;
using AnimatedControls.Avalonia;
using Avalonia.Controls;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Controls;

public static class Source
{
    public static readonly AttachedProperty<string?> CacheProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>(
            "Cache",
            typeof(Source),
            defaultValue: null);

    public static readonly AttachedProperty<bool> LoadedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "Loaded",
            typeof(Source),
            defaultValue: false);

    static Source()
    {
        CacheProperty.Changed.AddClassHandler<AnimatedImage>(OnAnimatedImageChanged);
        CacheProperty.Changed.AddClassHandler<AvatarImage>(OnAvatarImageChanged);
        CacheProperty.Changed.AddClassHandler<Image>(OnImageChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetCache(Control element) => element.GetValue(CacheProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetCache(Control element, string? value) => element.SetValue(CacheProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetLoaded(Control element) => element.GetValue(LoadedProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetLoaded(Control element, bool value) => element.SetValue(LoadedProperty, value);

    private static async void OnAnimatedImageChanged(AnimatedImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            SetLoaded(element, false);
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapFromCacheAsync(value);
        
        if (GetCache(element) == value)
        {
            var source = element.Source;
            element.Source = bitmap;
            SetLoaded(element, true);
            // Bitmap 可以多次 Dispose
            source?.Dispose();
        }

        element.RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, bitmap));
    }

    private static async void OnAvatarImageChanged(AvatarImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            SetLoaded(element, false);
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapFromCacheAsync(value);

        if (GetCache(element) == value)
        {
            var source = element.Source;
            element.Source = bitmap;
            SetLoaded(element, true);
            source?.Dispose();
        }

        element.RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, bitmap));
    }

    private static async void OnImageChanged(Image element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            SetLoaded(element, false);
            return;
        }

        var bitmap = await CacheHelper.GetBitmapFromCacheAsync(value);

        if (GetCache(element) == value)
        {
            var source = element.Source;
            element.Source = bitmap;
            SetLoaded(element, true);
            if (source is IDisposable disposable)
                disposable.Dispose();
        }

        element.RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, bitmap));
    }
}
