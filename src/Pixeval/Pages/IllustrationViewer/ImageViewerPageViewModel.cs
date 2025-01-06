#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ImageViewerPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.UserProfile;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Database.Managers;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Download;
using Pixeval.Util.ComponentModels;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions;
using Pixeval.Extensions.Common;
using Pixeval.Util.IO.Caching;
using Pixeval.Extensions.Common.Transformers;

namespace Pixeval.Pages.IllustrationViewer;

public partial class ImageViewerPageViewModel : UiObservableObject, IDisposable
{
    private bool _disposed;

    [ObservableProperty]
    public partial bool IsMirrored { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; } = true;

    [ObservableProperty]
    public partial double LoadingProgress { get; set; }

    [ObservableProperty]
    public partial string? LoadingText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<int>? MsIntervals { get; set; }

    /// <summary>
    /// 只有动态zip时才会是<see cref="Stream"/>，其他情况都是<see cref="IReadOnlyList{T}"/>
    /// </summary>
    [ObservableProperty]
    public partial object? OriginalStreamsSource { get; private set; }

    public ImageSource? ThumbnailSource => IllustrationViewModel.ThumbnailSource;

    [ObservableProperty]
    public partial int RotationDegree { get; set; }

    [ObservableProperty]
    public partial float Scale { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFit))]
    public partial ZoomableImageMode ShowMode { get; set; }

    /// <summary>
    /// 由于多窗口，可能在加载图片后改变设置，所以此处缓存原图设置
    /// </summary>
    private bool _isOriginal;

    /// <summary>
    /// <see cref="ShowMode"/> is <see cref="ZoomableImageMode.Fit"/> or not
    /// </summary>
    public bool IsFit => ShowMode is ZoomableImageMode.Fit;

    [ObservableProperty]
    public partial bool LoadSuccessfully { get; private set; }

    public ImageViewerPageViewModel(IllustrationItemViewModel illustrationViewModel, IllustrationItemViewModel originalIllustrationViewModel, ulong hWnd) : base(hWnd)
    {
        IllustrationViewModel = illustrationViewModel;
        OriginalIllustrationViewModel = originalIllustrationViewModel;
        _ = LoadImage();

        InitializeCommands();
        // 此时IsPlaying为true，IsFit为true
        PlayGifCommand.RefreshPlayCommand(true);
        RestoreResolutionCommand.RefreshResolutionCommand(true);
    }

    /// <summary>
    /// 如果之前下载的图片就是原图，则可以直接返回下载的图片
    /// </summary>
    public async ValueTask<IReadOnlyList<Stream>?> GetImageStreamsAsync(bool needOriginal)
    {
        if (needOriginal && !_isOriginal)
            return null;

        if (OriginalStreamsSource is null)
            return null;

        IReadOnlyList<Stream> ret;
        // 非原图的动图是ZIP格式
        switch (OriginalStreamsSource)
        {
            case IReadOnlyList<Stream> streams:
                ret = streams;
                break;
            case Stream stream:
                ret = await Streams.ReadZipAsync(stream, false);
                break;
            default:
                return null;
        }

        foreach (var s in ret)
            s.Position = 0;

        return ret;
    }

    public async Task GetOriginalStreamsSourceAsync(Stream destination, IProgress<double>? progress = null)
    {
        if (OriginalStreamsSource is not { } s)
            return;

        switch (s)
        {
            case IReadOnlyList<Stream> and [{ } stream]:
                stream.Position = 0;
                await stream.CopyToAsync(destination);
                break;
            case IReadOnlyList<Stream> streams when IllustrationViewModel.IsUgoira:
                _ = await streams.UgoiraSaveToStreamAsync(MsIntervals ?? [], destination, progress);
                return;
            case Stream stream:
                var list = await Streams.ReadZipAsync(stream, false);
                _ = await list.UgoiraSaveToStreamAsync(MsIntervals ?? [], destination, progress);
                break;
            default:
                return;
        }
    }

    public async Task<StorageFile> SaveToFolderAsync(AppKnownFolders appKnownFolder)
    {
        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var normalizedName = IoHelper.NormalizePathSegment(new IllustrationMetaPathParser().Reduce(name, IllustrationViewModel));
        normalizedName = IoHelper.ReplaceTokenExtensionFromUrl(normalizedName, IllustrationViewModel.IllustrationOriginalUrl);
        await using var stream = appKnownFolder.OpenAsyncWrite(normalizedName);
        await GetOriginalStreamsSourceAsync(stream);
        return await StorageFile.GetFileFromPathAsync(appKnownFolder.CombinePath(normalizedName));
    }

    public CancellationTokenSource ImageLoadingCancellationTokenSource { get; } = new();

    public IllustrationItemViewModel IllustrationViewModel { get; }

    public IllustrationItemViewModel OriginalIllustrationViewModel { get; }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    private void AdvancePhase(LoadingPhase phase, double progress = 0)
    {
        LoadingProgress = progress;
        LoadingText = phase switch
        {
            LoadingPhase.DownloadingImage => LoadingPhaseExtension.GetResource(LoadingPhase.DownloadingImage).Format((int)progress),
            _ => LoadingPhaseExtension.GetResource(phase)
        };
    }

    private async Task LoadImage()
    {
        if (LoadSuccessfully && !_disposed)
            return;
        _disposed = false;
        _ = LoadThumbnailAsync();
        BrowseHistoryPersistentManager.AddHistory(IllustrationViewModel.Entry);
        await LoadOriginalImageAsync();
        return;

        async Task LoadThumbnailAsync()
        {
            _ = await IllustrationViewModel.TryLoadThumbnailAsync(this);
            OnPropertyChanged(nameof(ThumbnailSource));
        }

        async Task LoadOriginalImageAsync()
        {
            var metadata = null as UgoiraMetadataResponse;
            if (IllustrationViewModel.IsUgoira)
                metadata = await IllustrationViewModel.UgoiraMetadata;

            _isOriginal = App.AppViewModel.AppSettings.BrowseOriginalImage;

            var ugoiraUrl = metadata?.LargeUrl;
            object? source = null;
            // 原图动图（一张一张下）
            if (ugoiraUrl is not null && _isOriginal)
            {
                var urls = await IllustrationViewModel.UgoiraOriginalUrlsAsync();
                var list = new List<Stream>();
                var ratio = 1d / urls.Count;
                var startProgress = 0d;
                foreach (var url in urls)
                {
                    if (await DownloadUrlAsync(url, startProgress, ratio) is { } stream)
                        list.Add(stream);
                    else
                    {
                        list = null;
                        break;
                    }

                    startProgress += 100 * ratio;
                }

                source = list;
            }
            else
            {
                // 静图
                if (ugoiraUrl is null)
                {
                    ugoiraUrl = IllustrationViewModel.StaticUrl(_isOriginal);
                    if (await DownloadUrlAsync(ugoiraUrl) is { } s)
                    {
                        var newStream = await s.CopyToMemoryStreamAsync(false);
                        s.Position = 0;
                        source = (IReadOnlyList<Stream>) [s];
                        ApplyImageTransformers(newStream);
                    }
                }
                // 非原图动图（压缩包）
                else
                {
                    source = await DownloadUrlAsync(ugoiraUrl);
                }

            }

            if (source is not null)
            {
                MsIntervals = metadata?.Delays.ToArray();
                OriginalStreamsSource = source;
            }

            LoadSuccessfully = true;

            if (OriginalStreamsSource is not null && !_disposed)
                UpdateCommandCanExecute();

            return;

            async Task<Stream?> DownloadUrlAsync(string url, double startProgress = 0, double ratio = 1)
            {
                AdvancePhase(LoadingPhase.CheckingCache);
                if (ImageLoadingCancellationTokenSource.IsCancellationRequested)
                    return null;
                return await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>()
                    .GetStreamFromMemoryCacheAsync(
                        url,
                        new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, startProgress + ratio * d)),
                        cancellationToken: ImageLoadingCancellationTokenSource.Token);
            }

            // 传入的s一定是新建的流，所以最后都会销毁s
            async void ApplyImageTransformers(Stream s)
            {
                var extensionService = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
                if (!extensionService.ActiveImageTransformers.Any())
                    return;
                var iStream = s.ToIStream();
                var isIllustrationOrFirstPageManga = IllustrationViewModel.MangaIndex is -1 or 0;
                var index = 1;
                var token = ImageLoadingCancellationTokenSource.Token;
                foreach (var transformer in extensionService.ActiveImageTransformers)
                {
                    if (token.IsCancellationRequested)
                        break;
                    if (isIllustrationOrFirstPageManga)
                        HWnd.InfoGrowl(ImageViewerPageResources.ApplyingTransformerExtensionsFormatted.Format(index));
                    // 运行扩展
                    iStream.Seek(0, SeekOrigin.Begin, out _);
                    var next = await transformer.TransformAsync(iStream);
                    if (next is null)
                        continue;
                    // 最初的s会在最后一行销毁，从别的扩展拿到的iStream无法销毁，所以此处直接覆盖之前的流的行为没有问题
                    iStream = next;
                    // 下一次循环准备
                    var newStream = iStream.ToStream();
                    var memoryStream = await newStream.CopyToMemoryStreamAsync(false);
                    iStream.Seek(0, SeekOrigin.Begin, out _);
                    // 显示图片
                    var last = OriginalStreamsSource;
                    OriginalStreamsSource = (IReadOnlyList<Stream>)[memoryStream];
                    if (last is IReadOnlyList<Stream> and [IDisposable disposable])
                        disposable.Dispose();
                    ++index;
                }
                if (!token.IsCancellationRequested && isIllustrationOrFirstPageManga)
                    HWnd.SuccessGrowl(ImageViewerPageResources.AllTransformerExtensionsFinished);
                await s.DisposeAsync();
            }
        }
    }

    private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsPlaying = !IsPlaying;
        PlayGifCommand.RefreshPlayCommand(IsPlaying);
    }

    public void RestoreResolutionCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ShowMode = IsFit ? ZoomableImageMode.Original : ZoomableImageMode.Fit;
        RestoreResolutionCommand.RefreshResolutionCommand(IsFit);
    }

    private void SetAsBackgroundCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SetPersonalization(UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync);
    }

    private void SetAsLockScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SetPersonalization(UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync);
    }

    private async void SetPersonalization(Func<StorageFile, IAsyncOperation<bool>> operation)
    {
        // TODO: 这里是否应该用原图设置？
        if (OriginalStreamsSource is null)
            return;

        var file = await SaveToFolderAsync(AppKnownFolders.Wallpapers);
        _ = await operation(file);

        ToastNotificationHelper.ShowTextToastNotification(
            EntryViewerPageResources.SetAsSucceededTitle,
            EntryViewerPageResources.SetAsBackgroundSucceededTitle);
    }

    private void ShareCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        HWnd.ShowShareUi();
    }

    private void InitializeCommands()
    {
        PlayGifCommand.CanExecuteRequested += (_, e) => e.CanExecute = IllustrationViewModel.IsUgoira && LoadSuccessfully;
        PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

        // 相当于鼠标滚轮滚动10次，方便快速缩放
        ZoomOutCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomOutCommand.ExecuteRequested += (_, _) => Scale = ZoomableImage.Zoom(-1200, Scale);

        ZoomInCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomInCommand.ExecuteRequested += (_, _) => Scale = ZoomableImage.Zoom(1200, Scale);

        RotateClockwiseCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        RotateClockwiseCommand.ExecuteRequested += (_, _) => RotationDegree += 90;

        RotateCounterclockwiseCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        RotateCounterclockwiseCommand.ExecuteRequested += (_, _) => RotationDegree -= 90;

        MirrorCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        MirrorCommand.ExecuteRequested += (_, _) => IsMirrored = !IsMirrored;

        RestoreResolutionCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        RestoreResolutionCommand.ExecuteRequested += RestoreResolutionCommandOnExecuteRequested;

        ShareCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ShareCommand.ExecuteRequested += ShareCommandExecuteRequested;

        SetAsLockScreenCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsLockScreenCommand.ExecuteRequested += SetAsLockScreenCommandOnExecuteRequested;

        SetAsBackgroundCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsBackgroundCommand.ExecuteRequested += SetAsBackgroundCommandOnExecuteRequested;
    }

    private void UpdateCommandCanExecute()
    {
        PlayGifCommand.NotifyCanExecuteChanged();
        RestoreResolutionCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        RotateClockwiseCommand.NotifyCanExecuteChanged();
        RotateCounterclockwiseCommand.NotifyCanExecuteChanged();
        MirrorCommand.NotifyCanExecuteChanged();
        ShareCommand.NotifyCanExecuteChanged();
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand _, CanExecuteRequestedEventArgs args) => args.CanExecute = LoadSuccessfully;

    private void IsNotUgoiraAndLoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args) => args.CanExecute = !IllustrationViewModel.IsUgoira && LoadSuccessfully;

    public (ulong, GetImageStreams) DownloadParameter => (HWnd, GetImageStreamsAsync);

    public XamlUICommand PlayGifCommand { get; } = "".GetCommand(Symbol.Pause);

    public XamlUICommand ZoomOutCommand { get; } = EntryViewerPageResources.ZoomOut.GetCommand(
        Symbol.ZoomOut, VirtualKey.Subtract);

    public XamlUICommand ZoomInCommand { get; } = EntryViewerPageResources.ZoomIn.GetCommand(
        Symbol.ZoomIn, VirtualKey.Add);

    public XamlUICommand RotateClockwiseCommand { get; } = EntryViewerPageResources.RotateClockwise.GetCommand(
        Symbol.ArrowRotateClockwise, VirtualKeyModifiers.Control, VirtualKey.R);

    public XamlUICommand RotateCounterclockwiseCommand { get; } = EntryViewerPageResources.RotateCounterclockwise.GetCommand(
        Symbol.ArrowRotateCounterclockwise, VirtualKeyModifiers.Control, VirtualKey.L);

    public XamlUICommand MirrorCommand { get; } = EntryViewerPageResources.Mirror.GetCommand(
        Symbol.FlipHorizontal, VirtualKeyModifiers.Control, VirtualKey.M);

    public XamlUICommand RestoreResolutionCommand { get; } = "".GetCommand(Symbol.RatioOneToOne);

    public XamlUICommand ShareCommand { get; } = EntryViewerPageResources.Share.GetCommand(Symbol.Share);

    public XamlUICommand SetAsCommand { get; } = EntryViewerPageResources.SetAs.GetCommand(Symbol.PaintBrush);

    public XamlUICommand SetAsLockScreenCommand { get; } = new() { Label = EntryViewerPageResources.LockScreen };

    public XamlUICommand SetAsBackgroundCommand { get; } = new() { Label = EntryViewerPageResources.Background };

    private void DisposeInternal()
    {
        ImageLoadingCancellationTokenSource.TryCancelDispose();
        IllustrationViewModel.UnloadThumbnail(this);

        OriginalStreamsSource = null;
        
        LoadSuccessfully = false;
    }
}

[LocalizationMetadata(typeof(ImageViewerPageResources))]
public enum LoadingPhase
{
    [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.CheckingCache))]
    CheckingCache,

    [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingFromCache))]
    LoadingFromCache,

    [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.MergingUgoiraFrames))]
    MergingUgoiraFrames,

    [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.DownloadingImageFormatted), DownloadingImage)]
    DownloadingImage,

    [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingImage))]
    LoadingImage,
}
