// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
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

    private static readonly AttachedProperty<SourceLoadLifetime?> CacheLoadLifetimeProperty =
        AvaloniaProperty.RegisterAttached<Control, SourceLoadLifetime?>(
            "CacheLoadLifetime",
            typeof(Source),
            defaultValue: null);

    private static readonly AttachedProperty<SourceLoadLifetime?> BackgroundCacheLoadLifetimeProperty =
        AvaloniaProperty.RegisterAttached<Control, SourceLoadLifetime?>(
            "BackgroundCacheLoadLifetime",
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

    private static async void OnAnimatedImageChanged(AnimatedImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            element.Source = null;
            CancelLoad(element, CacheLoadLifetimeProperty);
            SetLoaded(element, false);
            return;
        }

        element.Source = null;
        SetLoaded(element, false);
        var lifetime = BeginLoad(element, CacheLoadLifetimeProperty);
        try
        {
            var bitmap = await CacheHelper.GetAnimatedBitmapAsync(GetPlatform(element), value, token: lifetime.Token);
            if (!lifetime.TrySetSource(bitmap))
                return;
            if (GetCache(element) != value)
            {
                lifetime.Dispose();
                return;
            }

            element.Source = bitmap;
            SetLoaded(element, true);
        }
        catch (OperationCanceledException) when (lifetime.Token.IsCancellationRequested)
        {
        }
    }

    private static async void OnAvatarImageChanged(AvatarImage element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            element.Source = null;
            CancelLoad(element, CacheLoadLifetimeProperty);
            SetLoaded(element, false);
            return;
        }

        element.Source = null;
        SetLoaded(element, false);
        var lifetime = BeginLoad(element, CacheLoadLifetimeProperty);
        try
        {
            var bitmap = await CacheHelper.GetAnimatedBitmapAsync(GetPlatform(element), value, token: lifetime.Token);
            if (!lifetime.TrySetSource(bitmap))
                return;
            if (GetCache(element) != value)
            {
                lifetime.Dispose();
                return;
            }

            element.Source = bitmap;
            SetLoaded(element, true);
        }
        catch (OperationCanceledException) when (lifetime.Token.IsCancellationRequested)
        {
        }
    }

    private static async void OnImageChanged(Image element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<string>() is not { } value)
        {
            element.Source = null;
            CancelLoad(element, CacheLoadLifetimeProperty);
            SetLoaded(element, false);
            return;
        }

        element.Source = null;
        SetLoaded(element, false);
        var lifetime = BeginLoad(element, CacheLoadLifetimeProperty);
        try
        {
            var bitmap = await CacheHelper.GetBitmapAsync(GetPlatform(element), value, token: lifetime.Token);
            if (!lifetime.TrySetSource(bitmap))
                return;
            if (GetCache(element) != value)
            {
                lifetime.Dispose();
                return;
            }

            element.Source = bitmap;
            SetLoaded(element, true);
        }
        catch (OperationCanceledException) when (lifetime.Token.IsCancellationRequested)
        {
        }
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
            element.ClearValue(backgroundProperty);
            CancelLoad(element, BackgroundCacheLoadLifetimeProperty);
            SetLoaded(element, false);
            return;
        }

        element.ClearValue(backgroundProperty);
        SetLoaded(element, false);
        var lifetime = BeginLoad(element, BackgroundCacheLoadLifetimeProperty);
        try
        {
            var bitmap = await CacheHelper.GetBitmapAsync(GetPlatform(element), value, token: lifetime.Token);
            if (!lifetime.TrySetSource(bitmap))
                return;
            if (GetBackgroundCache(element) != value)
            {
                lifetime.Dispose();
                return;
            }

            var o = element.GetValue(backgroundProperty);
            if (o is ImageBrush brush)
            {
                brush.Source = bitmap;
            }
            else
            {
                brush = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            }

            _ = element.SetValue(backgroundProperty, brush);
            SetLoaded(element, true);
        }
        catch (OperationCanceledException) when (lifetime.Token.IsCancellationRequested)
        {
        }
    }

    private static SourceLoadOperation BeginLoad(
        Control element,
        AttachedProperty<SourceLoadLifetime?> lifetimeProperty)
    {
        if (element.GetValue(lifetimeProperty) is not { } lifetime)
        {
            lifetime = new SourceLoadLifetime();
            element.SetValue(lifetimeProperty, lifetime);
        }

        TryRegisterLifetime(element, lifetime);
        UpdateRegistrationRetry(element);
        return lifetime.BeginLoad();
    }

    private static void CancelLoad(
        Control element,
        AttachedProperty<SourceLoadLifetime?> lifetimeProperty)
    {
        element.GetValue(lifetimeProperty)?.CancelCurrent();
    }

    private static void OnElementLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
            return;

        if (element.GetValue(CacheLoadLifetimeProperty) is { } cacheLifetime)
            TryRegisterLifetime(element, cacheLifetime);
        if (element.GetValue(BackgroundCacheLoadLifetimeProperty) is { } backgroundLifetime)
            TryRegisterLifetime(element, backgroundLifetime);
        UpdateRegistrationRetry(element);
    }

    private static void TryRegisterLifetime(Control element, SourceLoadLifetime lifetime)
    {
        if (lifetime.IsRegistered || lifetime.IsDisposed)
            return;

        var args = new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, lifetime);
        element.RaiseEvent(args);
        if (args.Handled)
            lifetime.MarkRegistered();
    }

    private static void UpdateRegistrationRetry(Control element)
    {
        element.Loaded -= OnElementLoaded;
        if (RequiresRegistration(element.GetValue(CacheLoadLifetimeProperty))
            || RequiresRegistration(element.GetValue(BackgroundCacheLoadLifetimeProperty)))
            element.Loaded += OnElementLoaded;
        return;

        static bool RequiresRegistration(SourceLoadLifetime? lifetime) =>
            lifetime is { IsRegistered: false, IsDisposed: false };
    }
}

internal sealed class SourceLoadLifetime : IDisposable
{
    private readonly Lock _gate = new();
    private SourceLoadOperation? _current;

    public bool IsRegistered { get; private set; }

    public bool IsDisposed { get; private set; }

    public void MarkRegistered() => IsRegistered = true;

    public SourceLoadOperation BeginLoad()
    {
        var operation = new SourceLoadOperation();
        SourceLoadOperation? previous;
        lock (_gate)
        {
            if (IsDisposed)
            {
                operation.Dispose();
                return operation;
            }

            previous = _current;
            _current = operation;
        }

        previous?.Dispose();
        return operation;
    }

    public void CancelCurrent()
    {
        SourceLoadOperation? current;
        lock (_gate)
        {
            current = _current;
            _current = null;
        }

        current?.Dispose();
    }

    public void Dispose()
    {
        SourceLoadOperation? current;
        lock (_gate)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            current = _current;
            _current = null;
        }

        current?.Dispose();
    }
}

internal sealed class SourceLoadOperation : IDisposable
{
    private readonly Lock _gate = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IDisposable? _source;
    private bool _isDisposed;

    public SourceLoadOperation()
    {
        Token = _cancellationTokenSource.Token;
    }

    public CancellationToken Token { get; }

    public bool TrySetSource(IDisposable source)
    {
        lock (_gate)
        {
            if (_isDisposed)
            {
                source.Dispose();
                return false;
            }

            _source = source;
            return true;
        }
    }

    public void Dispose()
    {
        IDisposable? source;
        lock (_gate)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            source = _source;
            _source = null;
        }

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        source?.Dispose();
    }
}
