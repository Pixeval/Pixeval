// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Controls;

internal sealed class SizedImageLoader : IDisposable
{
    private readonly record struct SourceId(string? Key, string? Platform);

    private readonly Image _image;
    private readonly Control _target;
    private readonly SourceLoadLifetime _lifetime = new();
    private readonly DispatcherTimer _resizeTimer = new() { Interval = TimeSpan.FromMilliseconds(120) };

    private TopLevel? _topLevel;
    private SourceLoadOperation? _displayed;
    private SourceLoadOperation? _pending;
    private SourceId _source;
    private PixelSize _requestedSize;
    private PixelSize _loadedSize;
    private bool _registered;
    private bool _disposed;

    public SizedImageLoader(Image image, Control target)
    {
        _image = image;
        _target = target;

        _image.Loaded += OnLoaded;
        _image.Unloaded += OnUnloaded;
        _target.SizeChanged += OnSizeChanged;
        _resizeTimer.Tick += OnResizeTick;

        if (_image.IsLoaded)
            Attach();
    }

    public void UpdateRequest()
    {
        if (_disposed)
            return;

        var next = new SourceId(Source.GetCache(_image), Source.GetPlatform(_image));
        if (_source != next)
        {
            Clear();
            _source = next;
        }

        if (_source.Key is not null)
            QueueUpdate();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Attach();
        UpdateRequest();
    }

    private void Attach()
    {
        if (_disposed)
            return;

        if (!_registered)
        {
            var args = new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this);
            _image.RaiseEvent(args);
            _registered = args.Handled;
        }

        _topLevel = TopLevel.GetTopLevel(_image);
        if (_topLevel is not null)
        {
            _topLevel.ScalingChanged -= OnSizeChanged;
            _topLevel.ScalingChanged += OnSizeChanged;
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_topLevel is not null)
            _topLevel.ScalingChanged -= OnSizeChanged;
        _topLevel = null;
        Clear();
    }

    private void QueueUpdate()
    {
        _target.LayoutUpdated -= OnLayoutUpdated;
        _target.LayoutUpdated += OnLayoutUpdated;
        Dispatcher.UIThread.Post(UpdateSize, DispatcherPriority.Loaded);
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) => UpdateSize();

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (_disposed || _source.Key is null || !_image.IsLoaded)
            return;

        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    private void OnResizeTick(object? sender, EventArgs e)
    {
        _resizeTimer.Stop();
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (_disposed || _source.Key is null || !_image.IsLoaded
            || !_target.IsArrangeValid || !_image.IsArrangeValid)
            return;

        var size = _target.Bounds.Size;
        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height)
            || size.Width <= 0 || size.Height <= 0)
            return;

        _target.LayoutUpdated -= OnLayoutUpdated;

        var scale = _topLevel?.RenderScaling ?? 1;
        var width = RoundUpToMultiple(size.Width * scale, 32);
        var requested = new PixelSize(
            width,
            Math.Max(1, (int)Math.Ceiling(size.Height * width / size.Width)));

        if (_requestedSize != requested)
            _pending?.Abandon();

        _requestedSize = requested;

        if (_pending is null && _requestedSize != _loadedSize)
            _ = LoadAsync();
    }

    private static int RoundUpToMultiple(double value, int multiple)
        => Math.Max(1, (int)Math.Ceiling(value / multiple) * multiple);

    private async Task LoadAsync()
    {
        var size = _requestedSize;
        // 保留当前显示，直到新结果可替换；同 URL 的尺寸变化串行，重绑卡片不等待旧下载。
        var operation = _pending = _lifetime.BeginLoad(preserveCurrentSource: true);

        try
        {
            var bitmap = await CacheHelper.GetBitmapAsync(
                _source.Platform!,
                _source.Key!,
                desiredSize: size,
                decodeToken: operation.DecodeToken,
                token: operation.Token);

            if (!operation.TrySetSource(bitmap))
                return;

            _image.Source = bitmap;
            _loadedSize = size;
            Source.SetLoaded(_image, true);

            _displayed?.Abandon();
            _displayed = operation;
        }
        catch (OperationCanceledException) when (operation.Token.IsCancellationRequested
                                                 || operation.DecodeToken.IsCancellationRequested)
        {
            operation.Dispose();
        }
        finally
        {
            if (ReferenceEquals(_pending, operation))
            {
                _pending = null;
                // 被取消的请求即使尺寸回退也不再使用旧结果
                if (!_disposed && _source.Key is not null && _image.IsLoaded && _requestedSize != _loadedSize)
                    QueueUpdate();
            }
        }
    }

    private void Clear()
    {
        _resizeTimer.Stop();
        _target.LayoutUpdated -= OnLayoutUpdated;

        _image.Source = null;
        Source.SetLoaded(_image, false);

        _pending?.Abandon();
        _pending = null;
        _displayed?.Abandon();
        _displayed = null;

        _requestedSize = _loadedSize = default;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Clear();

        _image.Loaded -= OnLoaded;
        _image.Unloaded -= OnUnloaded;
        _target.SizeChanged -= OnSizeChanged;
        _resizeTimer.Tick -= OnResizeTick;

        if (_topLevel is not null)
            _topLevel.ScalingChanged -= OnSizeChanged;

        _lifetime.Dispose();
    }
}
