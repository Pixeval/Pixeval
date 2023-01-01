#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ImageViewerPageViewModel.cs
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
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Attributes;
using Pixeval.CoreApi.Net;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public partial class ImageViewerPageViewModel : ObservableObject, IDisposable
{
    public enum LoadingPhase
    {
        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.CheckingCache))]
        CheckingCache,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingFromCache))]
        LoadingFromCache,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.DownloadingGifZipFormatted), DownloadingGifZip)]
        DownloadingGifZip,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.MergingGifFrames))]
        MergingGifFrames,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.DownloadingImageFormatted), DownloadingImage)]
        DownloadingImage,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingImage))]
        LoadingImage
    }

    private const int MaxZoomFactor = 8;

    private const int MinZoomFactor = 1;

    private bool _disposed;

    private TaskNotifier? _loadingOriginalSourceTask;

    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private string? _loadingText;

    [ObservableProperty]
    private ImageSource? _originalImageSource;

    [ObservableProperty]
    private double _scale = 1;

    public ImageViewerPageViewModel(IllustrationViewerPageViewModel illustrationViewerPageViewModel, IllustrationViewModel illustrationViewModel)
    {
        IllustrationViewerPageViewModel = illustrationViewerPageViewModel;
        IllustrationViewModel = illustrationViewModel;
        ImageLoadingCancellationHandle = new CancellationHandle();
        _ = LoadImage();
    }


    public IRandomAccessStream? OriginalImageStream { get; private set; }

    public Task? LoadingOriginalSourceTask
    {
        get => _loadingOriginalSourceTask!;
        set => SetPropertyAndNotifyOnCompletion(ref _loadingOriginalSourceTask!, value);
    }

    public bool LoadingCompletedSuccessfully => LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false;

    public IllustrationViewModel IllustrationViewModel { get; }

    public CancellationHandle ImageLoadingCancellationHandle { get; private set; }

    /// <summary>
    ///     The view model of the <see cref="IllustrationViewerPage" /> that hosts the owner <see cref="ImageViewerPage" />
    ///     of this <see cref="ImageViewerPageViewModel" />
    /// </summary>
    public IllustrationViewerPageViewModel IllustrationViewerPageViewModel { get; }

    public void Dispose()
    {
        _disposed = true;
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    public void Zoom(double delta)
    {
        var factor = Scale;
        switch (delta)
        {
            case < 0 when factor > MinZoomFactor:
            case > 0 when factor < MaxZoomFactor:
                delta = (factor + delta) switch
                {
                    > MaxZoomFactor => MaxZoomFactor - factor,
                    < MinZoomFactor => -(factor - MinZoomFactor),
                    _ => delta
                };
                Scale += delta;
                break;
        }
    }

    private void AdvancePhase(LoadingPhase phase)
    {
        LoadingText = phase.GetLocalizedResource() switch
        {
            { FormatKey: LoadingPhase } attr => attr.GetLocalizedResourceContent()?.Format((int)LoadingProgress),
            var attr => attr?.GetLocalizedResourceContent()
        };
    }

    private void AddHistory()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        manager.Delete(x => x.Id == IllustrationViewerPageViewModel.IllustrationId);
        manager.Insert(new BrowseHistoryEntry { Id = IllustrationViewModel.Id });
    }

    public async Task LoadImage()
    {
        if (LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true } || _disposed)
        {
            _disposed = false;
            _ = IllustrationViewModel.LoadThumbnailIfRequired().ContinueWith(_ =>
            {
                OriginalImageSource ??= IllustrationViewModel.ThumbnailSource;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            AddHistory();
            await LoadOriginalImage();
        }
    }

    public async Task LoadOriginalImage()
    {
        var imageClient = App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
        var cacheKey = IllustrationViewModel.Illustration.GetIllustrationOriginalImageCacheKey();
        AdvancePhase(LoadingPhase.CheckingCache);
        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(cacheKey) is { } stream)
        {
            AdvancePhase(LoadingPhase.LoadingFromCache);
            OriginalImageStream = stream;
            OriginalImageSource = IllustrationViewModel.Illustration.IsUgoira()
                ? await OriginalImageStream.GetBitmapImageAsync(false)
                : await stream.EncodeSoftwareBitmapSourceAsync(false);
            LoadingOriginalSourceTask = Task.CompletedTask;
        }
        else if (IllustrationViewModel.IsUgoira)
        {
            var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(IllustrationViewModel.Id);
            if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url)
            {
                AdvancePhase(LoadingPhase.DownloadingGifZip);
                var downloadRes = await imageClient.DownloadAsStreamAsync(url, new Progress<int>(d =>
                {
                    LoadingProgress = d;
                    AdvancePhase(LoadingPhase.DownloadingGifZip);
                }), ImageLoadingCancellationHandle);
                switch (downloadRes)
                {
                    case Result<Stream>.Success(var zipStream):
                        AdvancePhase(LoadingPhase.MergingGifFrames);
                        OriginalImageStream = await IOHelper.GetGifStreamFromZipStreamAsync(zipStream, ugoiraMetadata);
                        break;
                    case Result<Stream>.Failure(OperationCanceledException):
                        return;
                }

                AdvancePhase(LoadingPhase.LoadingImage);
                OriginalImageSource = await OriginalImageStream!.GetBitmapImageAsync(false);
                LoadingOriginalSourceTask = Task.CompletedTask;
            }
        }
        else if (IllustrationViewModel.OriginalSourceUrl is { } src)
        {
            AdvancePhase(LoadingPhase.DownloadingImage);
            switch (await imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<int>(d =>
                    {
                        LoadingProgress = d;
                        AdvancePhase(LoadingPhase.DownloadingImage);
                    }), ImageLoadingCancellationHandle))
            {
                case Result<IRandomAccessStream>.Success(var s):
                    OriginalImageStream = s;
                    break;
                case Result<IRandomAccessStream>.Failure(OperationCanceledException):
                    return;
            }

            AdvancePhase(LoadingPhase.LoadingImage);
            OriginalImageSource = await OriginalImageStream!.EncodeSoftwareBitmapSourceAsync(false);
            LoadingOriginalSourceTask = Task.CompletedTask;
        }

        if (OriginalImageStream is not null && !_disposed)
        {
            IllustrationViewerPageViewModel.UpdateCommandCanExecute();
            if (App.AppViewModel.AppSetting.UseFileCache)
            {
                await App.AppViewModel.Cache.TryAddAsync(cacheKey, OriginalImageStream!, TimeSpan.FromDays(1));
            }

            return;
        }

        throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
    }

    /// <summary>
    ///     We use the <see cref="IllustrationViewerPageViewModel" /> to remove and add bookmark
    ///     because the manga have multiple works and those works aside of this one cannot receive
    ///     the bookmark notification if we use <see cref="IllustrationViewModel" />
    /// </summary>
    public void SwitchBookmarkState()
    {
        if (IllustrationViewerPageViewModel.FirstIllustrationViewModel?.IsBookmarked is true)
        {
            IllustrationViewerPageViewModel.RemoveBookmarkAsync();
        }
        else
        {
            IllustrationViewerPageViewModel.PostPublicBookmarkAsync();
        }
    }

    public Visibility GetLoadingMaskVisibility(Task? loadingTask)
    {
        return !(loadingTask?.IsCompletedSuccessfully ?? false) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DisposeInternal()
    {
        OriginalImageStream?.Dispose();
        // if the loading task is null or hasn't been completed yet, the 
        // OriginalImageSource would be the thumbnail source, its disposal may 
        // causing the IllustrationGrid shows weird result such as an empty content
        if (LoadingOriginalSourceTask?.IsCompletedSuccessfully is true)
        {
            (OriginalImageSource as SoftwareBitmapSource)?.Dispose();
        }

        OriginalImageSource = null;
        LoadingOriginalSourceTask?.Dispose();
    }
}
