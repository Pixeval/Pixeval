// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels.Viewers;

public partial class ImageViewerViewModel : ViewModelBase, IDisposable
{
    public ImageViewerViewModel(IllustrationItemViewModel thumbnailViewModel)
    {
        ThumbnailViewModel = thumbnailViewModel;
        var platform = thumbnailViewModel.Entry.Platform;

        var entry = thumbnailViewModel.Entry;

        Images = entry is not IImageSet set
            ? [new(platform, entry)]
            : set.Pages.Select(t => new SingleViewerViewModel(platform, t)).ToArray();

        PageCount = Images.Count;

        BrowseHistoryPersistentManager.AddHistory(thumbnailViewModel.Entry);
    }

    public IllustrationItemViewModel ThumbnailViewModel { get; set; }

    public IReadOnlyList<SingleViewerViewModelBase> Images { get; }

    [ObservableProperty]
    public partial int SelectedPageIndex { get; set; }

    public int PageCount { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var loadableBitmap in Images)
            loadableBitmap.Dispose();
    }
}

public abstract partial class SingleViewerViewModelBase : ViewModelBase, IDisposable
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
    public IAnimatedBitmap? DisplaySource => OriginalSource;

    /// <summary>
    /// 原图源（处理前的图片）
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySource))]
    [NotifyPropertyChangedFor(nameof(IsPicGif))]
    [NotifyPropertyChangedFor(nameof(IsGifLoadSuccessfully))]
    public partial IAnimatedBitmap? OriginalSource { get; private set; }

    public bool IsPicGif => DisplaySource is { IsFailed: false, IsInitialized: true, Frames.Count: > 1 };

    public bool IsGifLoadSuccessfully => LoadSuccessfully && IsPicGif;

    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        OriginalSource?.Dispose();
    }

    public async Task LoadOriginalImageAsync()
    {
        if (LoadSuccessfully && !_disposed)
            return;

        AdvancePhase(LoadingPhase.LoadingImage);

        var source = await LoadOriginalImageOverrideAsync();

        if (source is not null)
        {
            OriginalSource = source;
            LoadSuccessfully = true;
        }
    }

    public abstract Task<IAnimatedBitmap?> LoadOriginalImageOverrideAsync();

    protected void AdvancePhase(LoadingPhase phase, double progress = 0)
    {
        LoadingProgress = progress;
        LoadingText = phase is LoadingPhase.DownloadingImage
            ? I18NManager.GetResource(ImageViewerPageResources.DownloadingImageFormatted, (int) progress)
            : I18NManager.GetResource(phase);
    }
}

public partial class SingleViewerViewModel(string platform, IArtworkInfo entry) : SingleViewerViewModelBase, IPlatformInfo
{
    public string Platform { get; } = platform;

    public IArtworkInfo Entry { get; } = entry;

    public override async Task<IAnimatedBitmap?> LoadOriginalImageOverrideAsync()
    {
        var isOriginal = false;
        switch (Entry)
        {
            // 当下载图集的其中一张图片时，ImageType会为ImageSet
            case ISingleImage { ImageType: ImageType.SingleImage or ImageType.ImageSet } singleImage:
            {
                var f = isOriginal ? singleImage : singleImage.Thumbnails.PickMax();
                if (f is null)
                    return null;
                var stream = await CacheHelper.GetSingleImageAsync(
                    Platform,
                    f,
                    new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
                return IAnimatedBitmap.Load(stream, true);
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
                        var list = await CacheHelper.GetAnimatedImageSeparatedAsync(
                            Platform,
                            f,
                            new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
                        var arr1 = new Stream[list.Count];
                        var arr2 = new int[list.Count];
                        for (var i = 0; i < list.Count; ++i)
                            (arr1[i], arr2[i]) = list[i];
                        return IAnimatedBitmap.Load(arr1, arr2, true);
                    }
                    case SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile:
                    {
                        var stream = await CacheHelper.GetSingleImageAsync(
                            Platform,
                            f,
                            new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, d)));
                        if (f.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile)
                        {
                            var (streams, delays) = await IoHelper.SplitAnimatedImageStreamAsync(stream);
                            return IAnimatedBitmap.Load(streams, delays, true);
                        }

                        var zipImageDelays = f.ZipImageDelays!;
                        await zipImageDelays.TryPreloadListAsync(Platform);
                        var zip = await Streams.ReadZipAsync(stream, true);
                        return IAnimatedBitmap.Load(zip, zipImageDelays, true);
                    }
                }

                break;
            }
        }

        return null;
    }
}
