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
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Database.Managers;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Download;
using Pixeval.Util.ComponentModels;

namespace Pixeval.Pages.IllustrationViewer;

public partial class ImageViewerPageViewModel : UiObservableObject, IDisposable
{
    private bool _disposed;

    [ObservableProperty]
    private bool _isMirrored;

    [ObservableProperty]
    private bool _isPlaying = true;

    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private string? _loadingText;

    [ObservableProperty]
    private IReadOnlyList<int>? _msIntervals;

    [ObservableProperty]
    private IReadOnlyList<Stream>? _originalImageSources;

    [ObservableProperty]
    private int _rotationDegree;

    [ObservableProperty]
    private float _scale = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFit))]
    private ZoomableImageMode _showMode;

    /// <summary>
    /// 由于多窗口，可能在加载图片后改变设置，所以此处缓存原图设置
    /// </summary>
    private bool _isOriginal;

    /// <summary>
    /// <see cref="ShowMode"/> is <see cref="ZoomableImageMode.Fit"/> or not
    /// </summary>
    public bool IsFit => ShowMode is ZoomableImageMode.Fit;

    private bool _loadSuccessfully;

    public bool LoadSuccessfully
    {
        get => _loadSuccessfully;
        private set
        {
            if (value == _loadSuccessfully)
                return;
            _loadSuccessfully = value;
            OnPropertyChanged();
        }
    }

    public ImageViewerPageViewModel(IllustrationItemViewModel illustrationViewModel, IllustrationItemViewModel originalIllustrationViewModel, ulong hWnd) : base(hWnd)
    {
        IllustrationViewModel = illustrationViewModel;
        OriginalIllustrationViewModel = originalIllustrationViewModel;
        _ = LoadImage();

        InitializeCommands();
        // 此时IsPlaying为true，IsFit为true
        PlayGifCommand.GetPlayCommand(true);
        RestoreResolutionCommand.GetResolutionCommand(true);
    }

    /// <summary>
    /// 如果之前下载的图片就是原图，则可以直接返回下载的图片
    /// </summary>
    public IReadOnlyList<Stream>? GetImageStreams(bool needOriginal)
    {
        if (needOriginal && !_isOriginal)
            return null;

        if (OriginalImageSources is not [var stream, ..])
            return null;

        var ret = IllustrationViewModel.IsUgoira ? OriginalImageSources : [stream];

        foreach (var s in ret)
            s.Position = 0;
        return ret;
    }

    public async Task GetOriginalImageSourceAsync(Stream destination, IProgress<double>? progress = null)
    {
        if (OriginalImageSources is not [var stream, ..])
            return;

        if (IllustrationViewModel.IsUgoira)
        {
            _ = await OriginalImageSources.UgoiraSaveToStreamAsync(MsIntervals ?? [], destination, progress);
            return;
        }

        stream.Position = 0;
        await stream.CopyToAsync(destination);
    }

    public async Task<StorageFile> SaveToFolderAsync(AppKnownFolders appKnownFolder)
    {
        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = IoHelper.NormalizePathSegment(new IllustrationMetaPathParser().Reduce(name, IllustrationViewModel));
        path = IoHelper.ReplaceTokenExtensionFromUrl(path, IllustrationViewModel.IllustrationOriginalUrl);
        var file = await appKnownFolder.CreateFileAsync(path);
        await using var target = await file.OpenStreamForWriteAsync();
        await GetOriginalImageSourceAsync(target);
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
        if (phase is LoadingPhase.DownloadingImage)
            LoadingText = LoadingPhaseExtension.GetResource(LoadingPhase.DownloadingImage)
                ?.Format((int)(LoadingProgress = progress));
        else
            LoadingText = LoadingPhaseExtension.GetResource(phase);
    }

    private async Task LoadImage()
    {
        if (LoadSuccessfully && !_disposed)
            return;
        _disposed = false;
        _ = LoadThumbnailAsync();
        BrowseHistoryPersistentManager.AddHistory(IllustrationViewModel.Entry);
        await LoadOriginalImageAsync();
        IllustrationViewModel.UnloadThumbnail(this);
        return;

        async Task LoadThumbnailAsync()
        {
            _ = await IllustrationViewModel.TryLoadThumbnailAsync(this);
            OriginalImageSources ??= [IllustrationViewModel.ThumbnailStream!];
        }

        async Task<IReadOnlyList<Stream>?> GetStreamsAsync(string? ugoiraLargeUrl)
        {
            _isOriginal = App.AppViewModel.AppSettings.BrowseOriginalImage;

            if (ugoiraLargeUrl is null)
            {
                var url = IllustrationViewModel.StaticUrl(_isOriginal);
                if (await DownloadUrlAsync(url) is { } stream)
                    return [stream];
            }
            else
            {
                if (await DownloadUrlAsync(ugoiraLargeUrl) is { } downloadStream)
                    return await UnzipUgoiraAsync(downloadStream);

                if (_isOriginal)
                {
                    var urls = await IllustrationViewModel.UgoiraOriginalUrlsAsync();
                    var list = new List<Stream>();
                    var ratio = 1d / urls.Count;
                    var startProgress = 0d;
                    foreach (var url in urls)
                    {
                        var stream = await DownloadUrlAsync(url, startProgress, ratio);
                        if (stream is null)
                            return null;
                        list.Add(stream);
                        startProgress += 100 * ratio;
                    }

                    return list;
                }
            }

            return null;

            async Task<Stream[]> UnzipUgoiraAsync(Stream zipStream)
            {
                AdvancePhase(LoadingPhase.MergingUgoiraFrames);
                return await IoHelper.ReadZipAsync(zipStream, true);
            }

            async Task<Stream?> DownloadUrlAsync(string url, double startProgress = 0, double ratio = 1)
            {
                var cacheKey = _isOriginal ? MakoHelper.GetOriginalCacheKey(url) : MakoHelper.GetThumbnailCacheKey(url);
                AdvancePhase(LoadingPhase.CheckingCache);
                if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.ExistsAsync(cacheKey))
                {
                    AdvancePhase(LoadingPhase.LoadingFromCache);

                    // 就算已经有该Key，只要取不出值，就重新加载
                    if (await App.AppViewModel.Cache.TryGetAsync<Stream>(cacheKey) is { } stream)
                        return stream;
                }

                var downloadRes = await App.AppViewModel.MakoClient.DownloadMemoryStreamAsync(url, new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, startProgress + ratio * d)), ImageLoadingCancellationTokenSource.Token);
                if (downloadRes is Result<Stream>.Success(var stream2))
                {
                    if (App.AppViewModel.AppSettings.UseFileCache)
                    {
                        await App.AppViewModel.Cache.AddAsync(cacheKey, stream2, TimeSpan.FromDays(1));
                        stream2.Position = 0;
                    }

                    return stream2;
                }

                return null;
            }
        }

        async Task LoadOriginalImageAsync()
        {
            var metadata = null as UgoiraMetadataResponse;
            if (IllustrationViewModel.IsUgoira)
                metadata = await IllustrationViewModel.UgoiraMetadata.ValueAsync;

            var streams = await GetStreamsAsync(metadata?.LargeUrl);
            if (streams is not null)
            {
                if (IllustrationViewModel.IsUgoira)
                {
                    MsIntervals = metadata!.Delays.ToArray();
                }

                OriginalImageSources = streams;
            }

            LoadSuccessfully = true;

            if (OriginalImageSources is not null && !_disposed)
                UpdateCommandCanExecute();
        }
    }

    private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsPlaying = !IsPlaying;
        PlayGifCommand.GetPlayCommand(IsPlaying);
    }

    public void RestoreResolutionCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ShowMode = IsFit ? ZoomableImageMode.Original : ZoomableImageMode.Fit;
        RestoreResolutionCommand.GetResolutionCommand(IsFit);
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
        // 这里是否应该用原图设置？
        if (OriginalImageSources is not [not null, ..])
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

    public (ulong, GetImageStreams) DownloadParameter => (HWnd, GetImageStreams);

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
        ImageLoadingCancellationTokenSource.Cancel();
        ImageLoadingCancellationTokenSource.Dispose();
        IllustrationViewModel.UnloadThumbnail(this);
        // if the loading task is null or hasn't been completed yet, the 
        // OriginalImageSources would be the thumbnail source, its disposal may 
        // cause the IllustrationGrid shows weird result such as an empty content
        var temp = OriginalImageSources;
        OriginalImageSources = null;

        if (LoadSuccessfully && temp is not null)
            foreach (var originalImageSource in temp)
            {
                originalImageSource?.Dispose();
            }

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
