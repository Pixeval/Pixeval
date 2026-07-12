// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
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
            AbandonLoad(element, CacheLoadLifetimeProperty);
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
            AbandonLoad(element, CacheLoadLifetimeProperty);
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
            AbandonLoad(element, CacheLoadLifetimeProperty);
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
            AbandonLoad(element, BackgroundCacheLoadLifetimeProperty);
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

    private static void AbandonLoad(
        Control element,
        AttachedProperty<SourceLoadLifetime?> lifetimeProperty)
    {
        element.GetValue(lifetimeProperty)?.AbandonCurrent();
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
    private readonly HashSet<SourceLoadOperation> _operations = [];
    private SourceLoadOperation? _current;

    public bool IsRegistered { get; private set; }

    public bool IsDisposed { get; private set; }

    public void MarkRegistered() => IsRegistered = true;

    public SourceLoadOperation BeginLoad()
    {
        var operation = new SourceLoadOperation(OnOperationDisposed);
        SourceLoadOperation? previous = null;
        var isDisposed = false;
        lock (_gate)
        {
            if (IsDisposed)
                isDisposed = true;
            else
            {
                previous = _current;
                _current = operation;
                _operations.Add(operation);
            }
        }

        if (isDisposed)
        {
            operation.Dispose();
            return operation;
        }

        // Recycling a container only makes its old result obsolete. Let the cache fill finish so
        // virtualized scrolling doesn't turn every binding change into a canceled HTTP request.
        previous?.Abandon();
        return operation;
    }

    public void AbandonCurrent()
    {
        SourceLoadOperation? current;
        lock (_gate)
        {
            current = _current;
            _current = null;
        }

        current?.Abandon();
    }

    public void Dispose()
    {
        SourceLoadOperation[] operations;
        lock (_gate)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            _current = null;
            operations = [.. _operations];
            _operations.Clear();
        }

        foreach (var operation in operations)
            operation.Dispose();
    }

    private void OnOperationDisposed(SourceLoadOperation operation)
    {
        lock (_gate)
        {
            _operations.Remove(operation);
            if (ReferenceEquals(_current, operation))
                _current = null;
        }
    }
}

internal sealed class SourceLoadOperation : IDisposable
{
    private readonly Action<SourceLoadOperation> _onDisposed;
    private readonly Lock _gate = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IDisposable? _source;
    private bool _isAbandoned;
    private bool _isDisposed;

    public SourceLoadOperation(Action<SourceLoadOperation> onDisposed)
    {
        _onDisposed = onDisposed;
        Token = _cancellationTokenSource.Token;
    }

    public CancellationToken Token { get; }

    public bool TrySetSource(IDisposable source)
    {
        var shouldComplete = false;
        lock (_gate)
        {
            if (!_isDisposed && !_isAbandoned)
            {
                _source = source;
                return true;
            }

            if (!_isDisposed)
            {
                _isDisposed = true;
                shouldComplete = true;
            }
        }

        source.Dispose();
        if (shouldComplete)
            Complete(cancel: false);
        return false;
    }

    public void Abandon()
    {
        IDisposable? source;
        lock (_gate)
        {
            if (_isDisposed || _isAbandoned)
                return;

            _isAbandoned = true;
            source = _source;
            _source = null;
            if (source is null)
                return;

            _isDisposed = true;
        }

        source.Dispose();
        Complete(cancel: false);
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

        source?.Dispose();
        Complete(cancel: true);
    }

    private void Complete(bool cancel)
    {
        if (cancel)
            _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _onDisposed(this);
    }
}
