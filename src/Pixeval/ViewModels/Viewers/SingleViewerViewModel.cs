// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class SingleViewerViewModel : ViewModelBase, IDisposable
{
    public static ObservableCollection<IImageTransformerCommandExtension> TransformerExtensions { get; } = [.. ExtensionService.ActiveImageTransformerCommands];

    public static bool HasTransformerExtensions => TransformerExtensions.Count is not 0;

    // Multiple viewer interactions can request the same page concurrently; share one load task to keep progress stable.
    private readonly Lock _loadOriginalImageTaskGate = new();
    private Task? _loadOriginalImageTask;

    [ObservableProperty]
    public partial double LoadingProgress { get; private set; }

    [ObservableProperty]
    public partial string? LoadingText { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TransformExtensionCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(MirrorCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateClockwiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateCounterclockwiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomOutCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomToOriginalCommand))]
    public partial bool LoadSuccessfully { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TransformExtensionCommand))]
    public partial bool IsTransforming { get; private set; }

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

    public bool IsPicGif => _entry.ImageType is ImageType.SingleAnimatedImage;

    private bool IsGifLoadSuccessfully => LoadSuccessfully && IsPicGif;

    public bool CanTransformExtension => !IsPicGif && !IsTransforming && LoadSuccessfully && OriginalSource is not null;

    public IReadOnlyList<ImageTransformerExtensionCommandItem> TransformerExtensionItems { get; }

    public ImageTransformerExtensionCommandItem? PrimaryTransformerExtensionItem =>
        TransformerExtensionItems is [{ } first, ..] ? first : null;

    private bool _disposed;

    private bool _thumbnailLoaded;
    private readonly string _platform;
    private readonly IArtworkInfo _entry;

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
    public SingleViewerViewModel(string platform, IArtworkInfo entry, int index)
    {
        _platform = platform;
        _entry = entry;
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
        TransformedSource?.Dispose();
        OriginalSource?.Dispose();
        ThumbnailSource?.Dispose();
    }

    public async Task LoadThumbnailImageAsync()
    {
        if (_thumbnailLoaded || _disposed)
            return;

        _thumbnailLoaded = true;

        ThumbnailSource = await LoadThumbnailImageOverrideAsync();
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

        AdvancePhase(LoadingPhase.LoadingImage);

        var source = await LoadOriginalImageOverrideAsync();

        if (source is null)
            return;

        if (_disposed)
        {
            source.Dispose();
            return;
        }

        OriginalSource = source;
        LoadSuccessfully = true;
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

    private void AdvancePhase(LoadingPhase phase, double progress = 0)
    {
        LoadingProgress = progress;
        LoadingText = phase is LoadingPhase.DownloadingImage
            ? I18NManager.GetResource(ImageViewerPageResources.DownloadingImageFormatted, (int) progress)
            : I18NManager.GetResource(phase switch
            {
                LoadingPhase.CheckingCache => ImageViewerPageResources.CheckingCache,
                LoadingPhase.LoadingFromCache => ImageViewerPageResources.LoadingFromCache,
                LoadingPhase.MergingUgoiraFrames => ImageViewerPageResources.MergingUgoiraFrames,
                LoadingPhase.LoadingImage => ImageViewerPageResources.LoadingImage,
                _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
            });
    }

    public async Task<Bitmap?> LoadThumbnailImageOverrideAsync()
    {
        var f = _entry.Thumbnails.PickMax();
        if (f is null)
            return null;
        return await CacheHelper.GetBitmapAsync(
            _platform,
            f.ImageUri.OriginalString,
            null,
            100);
    }

    public async Task<IAnimatedBitmap?> LoadOriginalImageOverrideAsync()
    {
        var isOriginal = false; // TODO: 设置项
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
                    new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
            }
            case ISingleAnimatedImage { ImageType: ImageType.SingleAnimatedImage } singleAnimatedImage:
            {
                var f = isOriginal
                    ? singleAnimatedImage
                    : (await singleAnimatedImage.AnimatedThumbnails.ApplyAsync(t => t
                        .TryPreloadListAsync(singleAnimatedImage))).PickMax();
                if (f is null)
                    return null;
                switch (f.PreferredAnimatedImageType)
                {
                    case SingleAnimatedImageType.MultiFiles:
                    {
                        return await CacheHelper.GetAnimatedImageSeparatedAsync(
                            _platform,
                            f,
                            new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
                    }
                    case SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile:
                    {
                        return await CacheHelper.GetSingleAnimatedImageAsync(
                            _platform,
                            f,
                            new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
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

    internal static object CreateTransformerExtensionIcon(IImageTransformerCommandExtension extension) => new SymbolIcon
    {
        Symbol = Enum.IsDefined(extension.Icon) ? extension.Icon : Symbol.ImageEdit
    };

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

        IsTransforming = true;
        try
        {
            originalSource.Init();
            if (originalSource.Frames is not [var frame, ..])
                return;

            await using var source = Streams.RentStream();
            frame.Save(source);
            source.Position = 0;

            var destination = Streams.RentStream();
            try
            {
                await extension.TransformAsync(source, destination);
                destination.Position = 0;
                TransformedSource = IAnimatedBitmap.Load(destination, true);
            }
            catch
            {
                await destination.DisposeAsync();
                throw;
            }
        }
        finally
        {
            IsTransforming = false;
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
    private void PlayPause() => IsPlaying = !IsPlaying;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private async Task CopyAsync(Control control)
    {
        if (DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(control) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private async Task SaveAsync(Control control)
    {
        if (DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(control) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private async Task SaveAsAsync(Control control)
    {
        if (DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(control) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    private static ExtensionService ExtensionService => App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}

public enum LoadingPhase
{
    CheckingCache,
    LoadingFromCache,
    MergingUgoiraFrames,
    DownloadingImage,
    LoadingImage,
}

public sealed class ImageTransformerExtensionCommandItem
{
    public ImageTransformerExtensionCommandItem(SingleViewerViewModel viewModel, IImageTransformerCommandExtension extension)
    {
        Extension = extension;
        Label = extension.Label;
        Description = extension.Description;
        Icon = SingleViewerViewModel.CreateTransformerExtensionIcon(extension);
        Command = new AsyncRelayCommand<Control?>(
            control => viewModel.ExecuteTransformerExtensionAsync(extension, control),
            _ => viewModel.TransformExtensionCommand.CanExecute(extension));
        viewModel.TransformExtensionCommand.CanExecuteChanged += (_, _) => Command.NotifyCanExecuteChanged();
    }

    public IImageTransformerCommandExtension Extension { get; }

    public string Label { get; }

    public string Description { get; }

    public object Icon { get; }

    public IAsyncRelayCommand<Control?> Command { get; }
}
