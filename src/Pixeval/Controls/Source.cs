// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using AnimatedControls.Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Misaki;
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

    public static readonly AttachedProperty<string?> BackgroundCacheProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>(
            "BackgroundCache",
            typeof(Source),
            defaultValue: null);

    public static readonly AttachedProperty<bool> LoadedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "Loaded",
            typeof(Source),
            defaultValue: false);

    private static readonly AttachedProperty<IDisposable?> BackgroundCacheSourceProperty =
        AvaloniaProperty.RegisterAttached<Control, IDisposable?>(
            "BackgroundCacheSource",
            typeof(Source),
            defaultValue: null);

    // TODO: 如何确保Platform顺序高于Cache
    public static readonly AttachedProperty<string> PlatformProperty =
        AvaloniaProperty.RegisterAttached<Control, string>(
            "Platform",
            typeof(Source),
            defaultValue: IPlatformInfo.Pixiv);

    static Source()
    {
        _ = CacheProperty.Changed.AddClassHandler<AnimatedImage>(OnAnimatedImageChanged);
        _ = CacheProperty.Changed.AddClassHandler<AvatarImage>(OnAvatarImageChanged);
        _ = CacheProperty.Changed.AddClassHandler<Image>(OnImageChanged);

        _ = BackgroundCacheProperty.Changed.AddClassHandler<Border>(OnBorderBackgroundCacheChanged);
        _ = BackgroundCacheProperty.Changed.AddClassHandler<Panel>(OnPanelBackgroundCacheChanged);
        _ = BackgroundCacheProperty.Changed.AddClassHandler<TemplatedControl>(OnTemplatedControlBackgroundCacheChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetCache(Control element) => element.GetValue(CacheProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetCache(Control element, string? value) => element.SetValue(CacheProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string? GetBackgroundCache(Control element) => element.GetValue(BackgroundCacheProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetBackgroundCache(Control element, string? value) => element.SetValue(BackgroundCacheProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetLoaded(Control element) => element.GetValue(LoadedProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetLoaded(Control element, bool value) => element.SetValue(LoadedProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetPlatform(Control element) => element.GetValue(PlatformProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetPlatform(Control element, string value) => element.SetValue(PlatformProperty, value);

    private static IDisposable? GetBackgroundCacheSource(Control element) => element.GetValue(BackgroundCacheSourceProperty);

    private static void SetBackgroundCacheSource(Control element, IDisposable? value) => element.SetValue(BackgroundCacheSourceProperty, value);

    private static async void OnAnimatedImageChanged(AnimatedImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            SetLoaded(element, false);
            return;
        }

        var bitmap = await CacheHelper.GetAnimatedBitmapAsync(GetPlatform(element), value);
        
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

        var bitmap = await CacheHelper.GetAnimatedBitmapAsync(GetPlatform(element), value);

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

        var bitmap = await CacheHelper.GetBitmapAsync(GetPlatform(element), value);

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

    private static async void OnBorderBackgroundCacheChanged(Border element, AvaloniaPropertyChangedEventArgs e)
    {
        await OnBackgroundCacheChangedCore(
            element,
            e,
            Border.BackgroundProperty);
    }

    private static async void OnPanelBackgroundCacheChanged(Panel element, AvaloniaPropertyChangedEventArgs e)
    {
        await OnBackgroundCacheChangedCore(
            element,
            e,
            Panel.BackgroundProperty);
    }

    private static async void OnTemplatedControlBackgroundCacheChanged(TemplatedControl element, AvaloniaPropertyChangedEventArgs e)
    {
        await OnBackgroundCacheChangedCore(
            element,
            e,
            TemplatedControl.BackgroundProperty);
    }

    private static async Task OnBackgroundCacheChangedCore<TControl>(
        TControl element,
        AvaloniaPropertyChangedEventArgs e,
        AvaloniaProperty<IBrush?> backgroundProperty)
        where TControl : Control
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            SetLoaded(element, false);
            return;
        }

        var bitmap = await CacheHelper.GetBitmapAsync(GetPlatform(element), value);

        if (GetBackgroundCache(element) == value)
        {
            var previousSource = GetBackgroundCacheSource(element);

            var o = element.GetValue(backgroundProperty);
            if (o is ImageBrush brush)
            {
                (brush.Source as IDisposable)?.Dispose();
                brush.Source = bitmap;
            }
            else
            {
                brush = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            }

            _ = element.SetValue(backgroundProperty, brush);

            SetBackgroundCacheSource(element, bitmap);
            SetLoaded(element, true);

            if (!ReferenceEquals(previousSource, bitmap))
                previousSource?.Dispose();
        }

        element.RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, bitmap));
    }
}
