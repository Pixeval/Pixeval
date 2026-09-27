// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class SingleViewerViewModel : ViewModelBase, IDisposable
{
    public static ObservableCollection<IImageTransformerCommandExtension> TransformerExtensions { get; } = [.. ExtensionService.ActiveImageTransformerCommands];

    public static bool HasTransformerExtensions => TransformerExtensions.Count is not 0;

    // Multiple viewer interactions can request the same page concurrently; share one load task to keep progress stable.
    private readonly Lock _loadOriginalImageTaskGate = new();
    private readonly CancellationTokenSource _lifetimeCancellationTokenSource = new();
    private Task? _loadOriginalImageTask;
    private int _previewConsumers;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoadingIndeterminate))]
    public partial double? LoadingProgress { get; private set; }

    public bool IsLoadingIndeterminate => LoadingProgress is null;

    public bool IsLoading => !LoadSuccessfully || IsProcessingImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyCanExecuteChangedFor(nameof(TransformExtensionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ViewOriginalCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(MirrorCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateClockwiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateCounterclockwiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomOutCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomToOriginalCommand))]
    public partial bool LoadSuccessfully { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyCanExecuteChangedFor(nameof(TransformExtensionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ViewOriginalCommand))]
    public partial bool IsProcessingImage { get; private set; }

    /// <summary>
    /// 显示用图源
    /// </summary>
    public IAnimatedBitmap? DisplaySource => TransformedSource ?? OriginalSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySource))]
    public partial IAnimatedBitmap? TransformedSource { get; private set; }

    /// <summary>
    /// 原图源（处理前的图片）
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySource))]
    [NotifyCanExecuteChangedFor(nameof(TransformExtensionCommand))]
    public partial IAnimatedBitmap? OriginalSource { get; private set; }

    [ObservableProperty]
    public partial Bitmap? ThumbnailSource { get; private set; }

    [ObservableProperty]
    public partial Bitmap? CachedPreviewSource { get; private set; }

    private bool _cachedPreviewLoading;

    [ObservableProperty]
    public partial Bitmap? LoadingPreview { get; private set; }

    internal void AttachPreview()
    {
        if (Interlocked.Increment(ref _previewConsumers) is 1 && !LoadSuccessfully)
            _ = LoadCachedPreviewAsync();
    }

    private async Task LoadCachedPreviewAsync()
    {
        if (_cachedPreviewLoading || _disposed)
            return;
        _cachedPreviewLoading = true;
        try
        {
            var bitmap = await LoadThumbnailImageOverrideAsync(_lifetimeCancellationTokenSource.Token,
                ProgressiveImageDecoder.PreviewDimension);
            if (_disposed || LoadSuccessfully || _previewConsumers is 0)
                bitmap?.Dispose();
            else
                CachedPreviewSource = bitmap;
        }
        catch (OperationCanceledException) when (_lifetimeCancellationTokenSource.IsCancellationRequested)
        {
        }
        finally
        {
            _cachedPreviewLoading = false;
        }
    }

    internal void DetachPreview()
    {
        if (Interlocked.Decrement(ref _previewConsumers) is not 0)
            return;
        ReleaseLoadingPreview();
    }

    public bool IsPicGif => _entry.ImageType is ImageType.SingleAnimatedImage;

    private bool IsGifLoadSuccessfully => LoadSuccessfully && IsPicGif;

    public bool CanViewOriginal => !IsProcessingImage && LoadSuccessfully;

    public bool CanTransformExtension => !IsPicGif && CanViewOriginal && OriginalSource is not null;

    public IReadOnlyList<ImageTransformerExtensionCommandItem> TransformerExtensionItems { get; }

    public ImageTransformerExtensionCommandItem? PrimaryTransformerExtensionItem =>
        TransformerExtensionItems is [{ } first, ..] ? first : null;

    private bool _disposed;

    private bool _thumbnailLoaded;
    private readonly string _platform;
    private readonly IArtworkInfo _entry;
    private readonly Func<Control?, int, Task> _saveImageAsync;

    public int Index { get; }

    [ObservableProperty]
    public partial double ZoomFactor { get; set; } = 1;

    [ObservableProperty]
    public partial bool IsPlaying { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MirrorScaleX))]
    public partial bool IsMirrored { get; set; }

    [ObservableProperty]
    public partial int RotationDegree { get; set; }

    /// <summary>
    /// 镜像时为-1，否则为1
    /// </summary>
    public double MirrorScaleX => IsMirrored ? -1 : 1;

    /// <inheritdoc/>
    public SingleViewerViewModel(
        string platform,
        IArtworkInfo entry,
        int index,
        Func<Control?, int, Task> saveImageAsync)
    {
        _platform = platform;
        _entry = entry;
        _saveImageAsync = saveImageAsync;
        Index = index;
        TransformerExtensionItems = [.. TransformerExtensions.Select(extension => new ImageTransformerExtensionCommandItem(this, extension))];
        _ = LoadThumbnailImageAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _lifetimeCancellationTokenSource.Cancel();
        _lifetimeCancellationTokenSource.Dispose();
        TransformedSource = null;
        OriginalSource?.Dispose();
        ThumbnailSource?.Dispose();
        OriginalSource = null;
        ThumbnailSource = null;
        ReleaseLoadingPreview();
    }

    public async Task LoadThumbnailImageAsync()
    {
        if (_thumbnailLoaded || _disposed)
            return;

        _thumbnailLoaded = true;

        try
        {
            var source = await LoadThumbnailImageOverrideAsync(_lifetimeCancellationTokenSource.Token);
            if (_disposed)
            {
                source?.Dispose();
                return;
            }

            ThumbnailSource = source;
        }
        catch (OperationCanceledException) when (_lifetimeCancellationTokenSource.IsCancellationRequested)
        {
        }
    }

    public Task LoadOriginalImageAsync()
    {
        if (LoadSuccessfully || _disposed)
            return Task.CompletedTask;

        lock (_loadOriginalImageTaskGate)
        {
            if (_loadOriginalImageTask is { IsCompleted: false } loadingTask)
                return loadingTask;

            var newLoadingTask = LoadOriginalImageCoreAsync();
            _loadOriginalImageTask = newLoadingTask;
            _ = ResetLoadOriginalImageTaskAsync(newLoadingTask);
            return newLoadingTask;
        }
    }

    private async Task LoadOriginalImageCoreAsync()
    {
        if (LoadSuccessfully || _disposed)
            return;

        LoadingProgress = null;

        using var preview = new ProgressiveImagePreview(PublishLoadingPreviewAsync,
            () => Volatile.Read(ref _previewConsumers) > 0);
        IAnimatedBitmap? source;
        try
        {
            source = await LoadImageAsync(false, preview, _lifetimeCancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (_lifetimeCancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        if (source is null)
            return;

        if (_disposed)
        {
            source.Dispose();
            return;
        }

        preview.Dispose();
        OriginalSource?.Dispose();
        OriginalSource = source;
        // Keep the preview and progress visible while the full-resolution frames decode off-thread.
        await Task.Run(source.Init);
        if (_disposed)
            return;
        if (source.IsFailed)
        {
            source.Dispose();
            source = CacheHelper.AnimatedImageNotAvailable.Value;
            await Task.Run(source.Init);
            if (_disposed)
                return;
            OriginalSource = source;
        }
        LoadSuccessfully = source.IsInitialized;
    }

    internal void ReleaseLoadingPreview()
    {
        var cached = CachedPreviewSource;
        CachedPreviewSource = null;
        cached?.Dispose();
        var preview = LoadingPreview;
        LoadingPreview = null;
        preview?.Dispose();
    }

    private async Task PublishLoadingPreviewAsync(Bitmap bitmap)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_disposed || LoadSuccessfully || _previewConsumers is 0)
            {
                bitmap.Dispose();
                return;
            }

            var previous = LoadingPreview;
            LoadingPreview = bitmap;
            previous?.Dispose();
        });
    }

    private async Task ResetLoadOriginalImageTaskAsync(Task loadingTask)
    {
        try
        {
            await loadingTask;
        }
        catch
        {
            // The initiating caller observes the failure; this continuation only resets shared state.
        }
        finally
        {
            lock (_loadOriginalImageTaskGate)
            {
                if (ReferenceEquals(_loadOriginalImageTask, loadingTask))
                    _loadOriginalImageTask = null;
            }
        }
    }

    private void UpdateLoadingProgress(double progress)
    {
        if (_disposed)
            return;

        LoadingProgress = progress;
    }

    private async Task<Bitmap?> LoadThumbnailImageOverrideAsync(CancellationToken token, int maximumDimension = 100)
    {
        var candidates = _entry.Thumbnails.ToList();
        while (candidates.PickMax() is { } frame)
        {
            token.ThrowIfCancellationRequested();
            _ = candidates.Remove(frame);
            await using var stream = CacheHelper.TryGetStream(frame.ImageUri.OriginalString);
            if (stream is null)
                continue;

            // Reuse the largest already-downloaded thumbnail; never start a placeholder download.
            var width = frame is { Width: > 0, Height: > 0 }
                ? Math.Max(1, (int) (frame.Width * Math.Min(1d,
                    maximumDimension / (double) Math.Max(frame.Width, frame.Height))))
                : maximumDimension;
            try
            {
                return await stream.DecodeBitmapImageAsync(false, width);
            }
            catch (Exception) when (!token.IsCancellationRequested)
            {
                // A damaged cached variant should not prevent trying a smaller cached thumbnail.
            }
        }
        return null;
    }

    private async Task<IAnimatedBitmap?> LoadImageAsync(bool isOriginal, ProgressiveImagePreview? preview = null, CancellationToken token = default)
    {
        switch (_entry)
        {
            // 当下载图集的其中一张图片时，ImageType会为ImageSet
            case ISingleImage { ImageType: ImageType.SingleImage or ImageType.ImageSet } singleImage:
            {
                var f = isOriginal ? singleImage : singleImage.Thumbnails.PickMax();
                if (f is null)
                    return null;
                return await CacheHelper.GetSingleImageAsync(
                    _platform,
                    f,
                    new Progress<double>(UpdateLoadingProgress),
                    preview is null ? null : preview.UpdateAsync, token);
            }
            case ISingleAnimatedImage { ImageType: ImageType.SingleAnimatedImage } singleAnimatedImage:
            {
                var f = isOriginal
                    ? singleAnimatedImage
                    : (await singleAnimatedImage.AnimatedThumbnails.ApplyAsync(t => t
                        .TryPreloadListAsync(singleAnimatedImage, token: token))).PickMax();
                token.ThrowIfCancellationRequested();
                if (f is null)
                    return null;
                switch (f.PreferredAnimatedImageType)
                {
                    case SingleAnimatedImageType.MultiFiles:
                    {
                        return await CacheHelper.GetAnimatedImageSeparatedAsync(
                            _platform,
                            f,
                            new Progress<double>(UpdateLoadingProgress),
                            preview is null ? null : preview.UpdateAsync, token);
                    }
                    case SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile:
                    {
                        return await CacheHelper.GetSingleAnimatedImageAsync(
                            _platform,
                            f,
                            new Progress<double>(UpdateLoadingProgress),
                            preview is null ? null : f.PreferredAnimatedImageType is SingleAnimatedImageType.SingleZipFile
                                ? preview.UpdateZipAsync : preview.UpdateAsync, token);
                    }
                }

                break;
            }
        }

        return null;
    }

    partial void OnTransformedSourceChanging(IAnimatedBitmap? value)
    {
        if (!ReferenceEquals(value, TransformedSource))
            TransformedSource?.Dispose();
    }

    [RelayCommand(CanExecute = nameof(CanViewOriginal))]
    private async Task ViewOriginalAsync(Control? control)
    {
        var viewContainer = control is null ? null : TopLevel.GetTopLevel(control)?.ViewContainer;
        IsProcessingImage = true;
        try
        {
            viewContainer?.ShowInformation(I18NManager.GetResource(ImageViewerPageResources.LoadingOriginalImage));
            LoadingProgress = null;
            var source = await LoadImageAsync(true, token: _lifetimeCancellationTokenSource.Token);
            if (source is null)
            {
                viewContainer?.ShowError(I18NManager.GetResource(ImageViewerPageResources.OriginalImageLoadFailed));
                return;
            }

            if (_disposed)
            {
                source.Dispose();
                return;
            }

            TransformedSource = source;
            viewContainer?.ShowSuccess(I18NManager.GetResource(ImageViewerPageResources.OriginalImageLoadedSuccessfully));
        }
        catch (OperationCanceledException) when (_lifetimeCancellationTokenSource.IsCancellationRequested)
        {
        }
        catch
        {
            viewContainer?.ShowError(I18NManager.GetResource(ImageViewerPageResources.OriginalImageLoadFailed));
        }
        finally
        {
            IsProcessingImage = false;
        }
    }

    internal async Task ExecuteTransformerExtensionAsync(IImageTransformerCommandExtension extension, Control? control)
    {
        var viewContainer = control is null ? null : TopLevel.GetTopLevel(control)?.ViewContainer;
        try
        {
            viewContainer?.ShowInformation(I18NManager.GetResource(ImageViewerPageResources.ApplyingTransformerExtensions));
            await TransformExtensionCommand.ExecuteAsync(extension);
            viewContainer?.ShowSuccess(I18NManager.GetResource(ImageViewerPageResources.TransformerExtensionFinishedSuccessfully));
        }
        catch
        {
            viewContainer?.ShowError(I18NManager.GetResource(ImageViewerPageResources.TransformerExtensionFailed));
        }
    }

    [RelayCommand(CanExecute = nameof(CanTransformExtension))]
    private async Task TransformExtensionAsync(IImageTransformerCommandExtension? extension)
    {
        if (extension is null || OriginalSource is not { } originalSource)
            return;

        IsProcessingImage = true;
        try
        {
            originalSource.Init();
            if (originalSource.Frames is not [var frame, ..])
                return;

            await using var source = Streams.RentStream();
            frame.Save(source, new PngBitmapEncoderOptions());
            source.Position = 0;

            var destination = Streams.RentStream();
            try
            {
                await extension.TransformAsync(source, destination);
                destination.Position = 0;
                var transformedSource = IAnimatedBitmap.Load(destination, true);
                if (_disposed)
                    transformedSource.Dispose();
                else
                    TransformedSource = transformedSource;
            }
            catch
            {
                await destination.DisposeAsync();
                throw;
            }
        }
        finally
        {
            IsProcessingImage = false;
        }
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void ZoomIn() => ZoomFactor *= 1.2;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void ZoomOut() => ZoomFactor /= 1.2;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void ZoomToOriginal() => ZoomFactor = 1;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void Mirror()
    {
        // 仅做IsEnabled绑定，实际逻辑修改IsMirrored属性
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void RotateClockwise() => RotationDegree = (RotationDegree + 90) % 360;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void RotateCounterclockwise() => RotationDegree = (RotationDegree - 90 + 360) % 360;

    [RelayCommand(CanExecute = nameof(IsGifLoadSuccessfully))]
    private void PlayPause()
    {
        // 仅做IsEnabled绑定，实际逻辑修改IsPlaying属性
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private async Task CopyAsync(Control control)
    {
        if (DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(control) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        await clipboard.FlushAsync();
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    [RelayCommand]
    private Task SaveImageAsync(Control? control) => _saveImageAsync(control, Index);

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private async Task SaveAsAsync(Control control)
    {
        if (DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(control) is not
            { ViewContainer: { } viewContainer, StorageProvider: { } storageProvider })
            return;
        var file = await storageProvider.SaveFilePickerAsync(new()
        {
            FileTypeChoices =
            [
                new("PNG")
                {
                    Patterns = ["*.png"],
                    MimeTypes = ["image/png"]
                }
            ],
            DefaultExtension = "png",
            SuggestedFileName = _entry.Id
        });

        if (file is null)
            return;

        var stream = await file.OpenWriteAsync();
        singleFrame.Save(stream, new PngBitmapEncoderOptions());
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Saved), file.Path.OriginalString);
    }

    private static ExtensionService ExtensionService => App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}

public sealed class ImageTransformerExtensionCommandItem
{
    public ImageTransformerExtensionCommandItem(SingleViewerViewModel viewModel, IImageTransformerCommandExtension extension)
    {
        Extension = extension;
        Label = extension.Label;
        Description = extension.Description;
        Symbol = extension.Icon;
        Command = new AsyncRelayCommand<Control?>(
            control => viewModel.ExecuteTransformerExtensionAsync(extension, control),
            _ => viewModel.TransformExtensionCommand.CanExecute(extension));
        viewModel.TransformExtensionCommand.CanExecuteChanged += (_, _) => Command.NotifyCanExecuteChanged();
    }

    public IImageTransformerCommandExtension Extension { get; }

    public string Label { get; }

    public string Description { get; }

    public Symbol Symbol { get; }

    public IAsyncRelayCommand<Control?> Command { get; }
}
