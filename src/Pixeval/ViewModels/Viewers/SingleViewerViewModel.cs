// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels.Viewers;

public partial class SingleViewerViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial double LoadingProgress { get; private set; }

    [ObservableProperty]
    public partial string? LoadingText { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGifLoadSuccessfully))]
    public partial bool LoadSuccessfully { get; private set; }

    /// <summary>
    /// 显示用图源
    /// </summary>
    public IAnimatedBitmap? DisplaySource => OriginalSource; // TODO: 扩展用

    /// <summary>
    /// 原图源（处理前的图片）
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySource))]
    [NotifyPropertyChangedFor(nameof(IsPicGif))]
    [NotifyPropertyChangedFor(nameof(IsGifLoadSuccessfully))]
    public partial IAnimatedBitmap? OriginalSource { get; private set; }

    [ObservableProperty]
    public partial Bitmap? ThumbnailSource { get; private set; }

    public bool IsPicGif => DisplaySource is { IsFailed: false, IsInitialized: true, Frames.Count: > 1 };

    public bool IsGifLoadSuccessfully => LoadSuccessfully && IsPicGif;

    private bool _disposed;

    private bool _thumbnailLoaded;
    private readonly string _platform;
    private readonly IArtworkInfo _entry;

    /// <inheritdoc/>
    public SingleViewerViewModel(string platform, IArtworkInfo entry)
    {
        _platform = platform;
        _entry = entry;
        _ = LoadThumbnailImageAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
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

    public async Task LoadOriginalImageAsync()
    {
        if (LoadSuccessfully || _disposed)
            return;

        AdvancePhase(LoadingPhase.LoadingImage);

        var source = await LoadOriginalImageOverrideAsync();

        if (source is not null)
        {
            OriginalSource = source;
            LoadSuccessfully = true;
        }
    }

    protected void AdvancePhase(LoadingPhase phase, double progress = 0)
    {
        LoadingProgress = progress;
        LoadingText = phase is LoadingPhase.DownloadingImage
            ? I18NManager.GetResource(ImageViewerPageResources.DownloadingImageFormatted, (int) progress)
            : I18NManager.GetResource(phase);
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
}
