// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.AnimatedImage;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Viewers;

public partial class ImageViewerPageViewModel : ViewModelBase, IDisposable
{
    private bool _disposed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MirrorScaleX))]
    public partial bool IsMirrored { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; } = true;

    [ObservableProperty]
    public partial double LoadingProgress { get; set; }

    [ObservableProperty]
    public partial string? LoadingText { get; set; }

    /// <summary>
    /// 原图源（处理前的图片）
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySource))]
    public partial IAnimatedBitmap? OriginalSource { get; private set; }

    /// <summary>
    /// 显示用图源
    /// </summary>
    public IAnimatedBitmap? DisplaySource => OriginalSource;

    /// <summary>
    /// 缩略图
    /// </summary>
    public Bitmap? ThumbnailSource => IllustrationViewModel.Thumbnail;

    [ObservableProperty]
    public partial int RotationDegree { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateClockwiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RotateCounterclockwiseCommand))]
    public partial bool LoadSuccessfully { get; private set; }

    private bool LoadSuccessfullyIsGif => LoadSuccessfully && IllustrationViewModel.IsPicGif;

    private bool LoadSuccessfullyNotGif => LoadSuccessfully && !IllustrationViewModel.IsPicGif;

    /// <summary>
    /// 镜像时为-1，否则为1
    /// </summary>
    public double MirrorScaleX => IsMirrored ? -1 : 1;

    public IllustrationItemViewModel IllustrationViewModel { get; }

    public IllustrationItemViewModel OriginalIllustrationViewModel { get; }

    public CancellationTokenSource ImageLoadingCancellationTokenSource { get; } = new();

    public ImageViewerPageViewModel(
        IllustrationItemViewModel illustrationViewModel,
        IllustrationItemViewModel originalIllustrationViewModel)
    {
        IllustrationViewModel = illustrationViewModel;
        OriginalIllustrationViewModel = originalIllustrationViewModel;
        _ = LoadImageAsync();
    }

    private void AdvancePhase(LoadingPhase phase, double progress = 0)
    {
        LoadingProgress = progress;
        LoadingText = phase is LoadingPhase.DownloadingImage
            ? I18NManager.GetResource(ImageViewerPageResources.DownloadingImageFormatted, (int) progress)
            : I18NManager.GetResource(phase);
    }

    private async Task LoadImageAsync()
    {
        if (LoadSuccessfully && !_disposed)
            return;
        _disposed = false;

        _ = LoadThumbnailAsync();
        BrowseHistoryPersistentManager.AddHistory(IllustrationViewModel.Entry);
        // TODO i18n:加载图片而不是解码图片
        AdvancePhase(LoadingPhase.LoadingImage);

        var source = await IllustrationViewModel.LoadOriginalImageAsync(
            AdvancePhase, ImageLoadingCancellationTokenSource.Token);

        if (source is not null)
            OriginalSource = ConvertToAnimatedBitmap(source);

        LoadSuccessfully = true;

        return;

        async Task LoadThumbnailAsync()
        {
            _ = await IllustrationViewModel.TryLoadThumbnailAsync(this);
            OnPropertyChanged(nameof(ThumbnailSource));
        }
    }

    /// <summary>
    /// 将 <see cref="IllustrationViewModel.LoadOriginalImageAsync"/> 返回的原始对象
    /// 转换为 <see cref="AnimatedImage"/> 可用的 <see cref="IAnimatedBitmap"/>
    /// </summary>
    private static IAnimatedBitmap? ConvertToAnimatedBitmap(object? source) => source switch
    {
        Stream stream => IAnimatedBitmap.Load(stream, false),
        (IReadOnlyCollection<Stream> streams, IReadOnlyCollection<int> delays) =>
            IAnimatedBitmap.Load(streams, delays, false),
        (IReadOnlyCollection<Stream> streams, IEnumerable<int> delays) =>
            IAnimatedBitmap.Load(streams, [.. delays], false),
        _ => null
    };

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void RotateClockwise() => RotationDegree = (RotationDegree + 90) % 360;

    [RelayCommand(CanExecute = nameof(LoadSuccessfully))]
    private void RotateCounterclockwise() => RotationDegree = (RotationDegree - 90 + 360) % 360;

    [RelayCommand(CanExecute = nameof(LoadSuccessfullyNotGif))]
    private async Task CopyAsync(Control? parameter)
    {
        if (IllustrationViewModel.IsPicGif)
            return;
        if (TopLevel.GetTopLevel(parameter) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(DisplaySource?.Frames[0]);
        viewContainer?.ShowSuccess(I18NManager.GetResource(EntryItemResources.ImageSetToClipBoard));
    }

    [RelayCommand(CanExecute = nameof(LoadSuccessfullyIsGif))]
    private void PlayPause() => IsPlaying = !IsPlaying;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    private void DisposeInternal()
    {
        ImageLoadingCancellationTokenSource.Cancel();
        ImageLoadingCancellationTokenSource.Dispose();
        LoadSuccessfully = false;
        IllustrationViewModel.UnloadThumbnail(this);
        OriginalSource?.Dispose();
        OriginalSource = null;
    }
}
