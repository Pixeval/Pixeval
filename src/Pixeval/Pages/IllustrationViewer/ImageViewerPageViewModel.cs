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
using System.Threading.Channels;
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
using Pixeval.Upscaling;
using Pixeval.Util.ComponentModels;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Pages.IllustrationViewer;

public partial class ImageViewerPageViewModel : UiObservableObject, IDisposable
{
    private bool _disposed;

    private bool _upscaled;

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

    //[ObservableProperty]
    //public partial bool Upscaling { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFit))]
    public partial ZoomableImageMode ShowMode { get; set; }

    //private Upscaler? _upscaler;

    //public Channel<string> UpscalerMessageChannel { get; } = Channel.CreateUnbounded<string>();

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
        var path = IoHelper.NormalizePathSegment(new IllustrationMetaPathParser().Reduce(name, IllustrationViewModel));
        path = IoHelper.ReplaceTokenExtensionFromUrl(path, IllustrationViewModel.IllustrationOriginalUrl);
        var file = await appKnownFolder.CreateFileAsync(path);
        await using var target = await file.OpenStreamForWriteAsync();
        await GetOriginalStreamsSourceAsync(target);
        return file;
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
        LoadingText = phase is LoadingPhase.DownloadingImage
            ? LoadingPhaseExtension.GetResource(LoadingPhase.DownloadingImage).Format((int) (LoadingProgress = progress)) 
            : LoadingPhaseExtension.GetResource(phase);
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
            _ = await IllustrationViewModel.TryLoadThumbnailAsync();
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
                if (ugoiraUrl is null)
                {
                    ugoiraUrl = IllustrationViewModel.StaticUrl(_isOriginal);
                    if (await DownloadUrlAsync(ugoiraUrl) is { } s)
                        source = (IReadOnlyList<Stream>) [s];
                }
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

        var file = await SaveToFolderAsync(AppKnownFolders.SavedWallPaper);
        _ = await operation(file);

        ToastNotificationHelper.ShowTextToastNotification(
            EntryViewerPageResources.SetAsSucceededTitle,
            EntryViewerPageResources.SetAsBackgroundSucceededTitle);
    }

    private void ShareCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        HWnd.ShowShareUi();
    }

    private async void UpscaleCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        //_upscaled = true;
        //UpscaleCommand.NotifyCanExecuteChanged();
        //_upscaler ??= new Upscaler(new(OriginalStream!.Single(), 
        //    App.AppViewModel.AppSettings.UpscalerModel,
        //    App.AppViewModel.AppSettings.UpscalerScaleRatio,
        //    App.AppViewModel.AppSettings.UpscalerOutputType));
        //var newStream = await _upscaler.UpscaleAsync(UpscalerMessageChannel);
        //OriginalStream = [newStream];
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

        //UpscaleCommand.CanExecuteRequested += IsUpscaleCommandCanExecutedRequested;
        //UpscaleCommand.ExecuteRequested += UpscaleCommandOnExecuteRequested;
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
        //UpscaleCommand.NotifyCanExecuteChanged();
    }

    private void IsUpscaleCommandCanExecutedRequested(XamlUICommand _, CanExecuteRequestedEventArgs args) => 
        args.CanExecute = LoadSuccessfully && !IllustrationViewModel.IsUgoira && !_upscaled;

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

    public XamlUICommand UpscaleCommand { get; } = EntryItemResources.AIUpscale.GetCommand(Symbol.EyeTracking);

    private void DisposeInternal()
    {
        ImageLoadingCancellationTokenSource.TryCancelDispose();
        IllustrationViewModel.UnloadThumbnail();

        OriginalStreamsSource = null;
        
        //_upscaler?.DisposeAsync();
        //UpscalerMessageChannel.Writer.Complete();

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
    LoadingImage
}
