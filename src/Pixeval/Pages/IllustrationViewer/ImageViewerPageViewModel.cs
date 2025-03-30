// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls;
using Mako.Net.Response;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.UserProfile;
using WinUI3Utilities;

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
    /// 原图流（原来/处理前的图片，可以是缩略图）
    /// </summary>
    /// <remarks>
    /// 只有动图zip时才会是<see cref="Stream"/>，其他情况都是<see cref="IReadOnlyList{T}"/>
    /// </remarks>
    [ObservableProperty]
    public partial object? OriginalStreamsSource { get; private set; }

    partial void OnOriginalStreamsSourceChanged(object? value) => DisplayStreamsSource = value;

    /// <summary>
    /// 显示图流
    /// </summary>
    /// <remarks>
    /// 只有动图zip时才会是<see cref="Stream"/>，其他情况都是<see cref="IReadOnlyList{T}"/>
    /// </remarks>
    [ObservableProperty]
    public partial object? DisplayStreamsSource { get; private set; }

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

    /// <summary>
    /// <see langword="true"/>能用，<see langword="false"/>不能用
    /// </summary>
    private bool ExtensionRunningLock
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            UpdateExtensionCommandCanExecute();
        }
    } = true;

    public ImageViewerPageViewModel(IllustrationItemViewModel illustrationViewModel, IllustrationItemViewModel originalIllustrationViewModel, FrameworkElement frameworkElement) : base(frameworkElement)
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

    public async Task GetDisplayStreamsSourceAsync(Stream destination, IProgress<double>? progress = null)
    {
        if (DisplayStreamsSource is not { } s)
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
        await GetDisplayStreamsSourceAsync(stream);
        return await StorageFile.GetFileFromPathAsync(appKnownFolder.CombinePath(normalizedName));
    }

    public async Task SaveAsync(string destination)
    {
        destination = IoHelper.NormalizePath(new IllustrationMetaPathParser().Reduce(destination, IllustrationViewModel));
        destination = IoHelper.ReplaceTokenExtensionFromUrl(destination, IllustrationViewModel.IllustrationOriginalUrl);
        await using var stream = IoHelper.OpenAsyncWrite(destination);
        await GetDisplayStreamsSourceAsync(stream);
        FrameworkElement?.SuccessGrowl(EntryItemResources.Saved);
    }

    public ICommand GetTransformExtensionCommand(IImageTransformerCommandExtension extension)
    {
        var command = new XamlUICommand();
        command.CanExecuteRequested += ExtensionCanExecuteRequested;
        command.ExecuteRequested += OnCommandOnExecuteRequested;
        ExtensionCommands.Add(command);
        return command;

        async void OnCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (OriginalStreamsSource is not (IReadOnlyList<Stream> and [{ } stream]))
                return;
            ExtensionRunningLock = false;
            try
            {
                var transformer = args.Parameter.To<IImageTransformerCommandExtension>();
                var token = ImageLoadingCancellationTokenSource.Token;
                if (token.IsCancellationRequested)
                    return;
                FrameworkElement.InfoGrowl(ImageViewerPageResources.ApplyingTransformerExtensions);
                // 运行扩展
                stream.Position = 0;
                var result = await transformer.TransformAsync(stream.ToIStream());
                if (result is null)
                {
                    FrameworkElement.ErrorGrowl(ImageViewerPageResources.TransformerExtensionFailed);
                    return;
                }
                _ = result.Seek(0, SeekOrigin.Begin);
                var memoryStream = Streams.RentStream();
                await result.CopyToAsync(memoryStream.ToIStream());
                await result.DisposeAsync();
                stream.Position = 0;
                // 显示图片
                var last = DisplayStreamsSource;
                DisplayStreamsSource = (IReadOnlyList<Stream>) [memoryStream];
                if (last is IReadOnlyList<Stream> and [IDisposable disposable] && !ReferenceEquals(disposable, stream))
                    disposable.Dispose();
                if (!token.IsCancellationRequested)
                    FrameworkElement.SuccessGrowl(ImageViewerPageResources.TransformerExtensionFinishedSuccessfully);
            }
            finally
            {
                ExtensionRunningLock = true;
            }
        }
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
            LoadingPhase.DownloadingImage => LoadingPhaseExtension.GetResource(LoadingPhase.DownloadingImage).Format((int) progress),
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
                var urls = IllustrationViewModel.UgoiraOriginalUrls;
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
                        source = (IReadOnlyList<Stream>) [s];
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
                return await CacheHelper.GetStreamFromCacheAsync(
                        url,
                        new Progress<double>(d => AdvancePhase(LoadingPhase.DownloadingImage, startProgress + ratio * d)),
                        cancellationToken: ImageLoadingCancellationTokenSource.Token);
            }
        }
    }

    private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (DisplayStreamsSource is IReadOnlyList<Stream> and [{ } stream])
        {
            stream.Position = 0;
            await UiHelper.ClipboardSetBitmapAsync(stream);
            FrameworkElement?.SuccessGrowl(EntryItemResources.ImageSetToClipBoard);
        }
    }

    private async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) =>
        await SaveAsync(App.AppViewModel.AppSettings.DownloadPathMacro);

    private async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var folder = await FrameworkElement.OpenFolderPickerAsync();
        if (folder is null)
        {
            FrameworkElement.InfoGrowl(EntryItemResources.SaveAsCancelled);
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = Path.Combine(folder.Path, name);
        await SaveAsync(path);
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
        if (DisplayStreamsSource is null)
            return;

        var file = await SaveToFolderAsync(AppKnownFolders.Wallpapers);
        _ = await operation(file);

        AppNotificationHelper.ShowTextAppNotification(
            EntryViewerPageResources.SetAsSucceededTitle,
            EntryViewerPageResources.SetAsBackgroundSucceededTitle);
    }

    private void ShareCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        DataTransferManagerInterop.ShowShareUIForWindow((nint) Window.HWnd);
    }

    private void InitializeCommands()
    {
        CopyCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;

        SaveCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;

        SaveAsCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

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
        CopyCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
        PlayGifCommand.NotifyCanExecuteChanged();
        RestoreResolutionCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        RotateClockwiseCommand.NotifyCanExecuteChanged();
        RotateCounterclockwiseCommand.NotifyCanExecuteChanged();
        MirrorCommand.NotifyCanExecuteChanged();
        ShareCommand.NotifyCanExecuteChanged();
        foreach (var command in ExtensionCommands)
            command.NotifyCanExecuteChanged();
    }

    private void UpdateExtensionCommandCanExecute()
    {
        foreach (var command in ExtensionCommands)
            command.NotifyCanExecuteChanged();
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand _, CanExecuteRequestedEventArgs args) => args.CanExecute = LoadSuccessfully;

    private void IsNotUgoiraAndLoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args) => args.CanExecute = !IllustrationViewModel.IsUgoira && LoadSuccessfully;

    private void ExtensionCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args) => args.CanExecute = !IllustrationViewModel.IsUgoira && LoadSuccessfully && ExtensionRunningLock;

    public (FrameworkElement, GetImageStreams) DownloadParameter => (FrameworkElement, GetImageStreamsAsync);

    public XamlUICommand CopyCommand { get; } = EntryViewerPageResources.Copy.GetCommand(Symbol.Copy, VirtualKeyModifiers.Control, VirtualKey.C);
  
    public XamlUICommand SaveCommand { get; } = EntryViewerPageResources.Save.GetCommand(Symbol.Save);

    public XamlUICommand SaveAsCommand { get; } = EntryViewerPageResources.SaveAs.GetCommand(Symbol.SaveEdit);

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

    public List<XamlUICommand> ExtensionCommands { get; } = [];

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
