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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.UserControls.IllustrationView;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using Windows.Storage.Streams;
using Pixeval.Options;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Pixeval.Controls;

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

    private bool _disposed;

    private TaskNotifier? _loadingOriginalSourceTask;

    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private string? _loadingText;

    [ObservableProperty]
    private IEnumerable<IRandomAccessStream>? _originalImageSources;

    [ObservableProperty]
    private List<int>? _msIntervals;

    [ObservableProperty]
    private float _scale = 1;

    [ObservableProperty]
    private bool _isPlaying = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFit))]
    private ZoomableImageMode _showMode;

    /// <summary>
    /// <see cref="ShowMode"/> is <see cref="ZoomableImageMode.Fit"/> or not
    /// </summary>
    public bool IsFit => ShowMode is ZoomableImageMode.Fit;

    public ImageViewerPageViewModel(IllustrationViewerPageViewModel illustrationViewerPageViewModel, IllustrationViewModel illustrationViewModel)
    {
        IllustrationViewerPageViewModel = illustrationViewerPageViewModel;
        IllustrationViewModel = illustrationViewModel;
        _ = LoadImage();
    }

    public IRandomAccessStream? OriginalImageStream => OriginalImageSources?.FirstOrDefault();

    public Task? LoadingOriginalSourceTask
    {
        get => _loadingOriginalSourceTask!;
        set => SetPropertyAndNotifyOnCompletion(ref _loadingOriginalSourceTask!, value);
    }

    public bool LoadingCompletedSuccessfully => LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false;

    public CancellationHandle ImageLoadingCancellationHandle { get; } = new();

    public IllustrationViewModel IllustrationViewModel { get; }

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

    public void Zoom(float delta)
    {
        Scale = MathF.Exp(MathF.Log(Scale) + delta / 5000f);
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
        manager.Insert(new() { Id = IllustrationViewModel.Id });
    }

    private async Task LoadImage()
    {
        if (!LoadingCompletedSuccessfully || _disposed)
        {
            _disposed = false;
            const ThumbnailUrlOption option = ThumbnailUrlOption.Medium;
            _ = IllustrationViewModel.TryLoadThumbnail(this, option).ContinueWith(
                _ =>
                {
                    OriginalImageSources ??= new[] { IllustrationViewModel.ThumbnailStreams[option] };
                },
                TaskScheduler.FromCurrentSynchronizationContext());
            AddHistory();
            await LoadOriginalImage();
            IllustrationViewModel.UnloadThumbnail(this, option);
        }

        return;

        async Task LoadOriginalImage()
        {
            var imageClient = App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
            var cacheKey = IllustrationViewModel.Illustrate.GetIllustrationOriginalImageCacheKey();
            AdvancePhase(LoadingPhase.CheckingCache);
            if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.ExistsAsync(cacheKey))
            {
                AdvancePhase(LoadingPhase.LoadingFromCache);
                OriginalImageSources = IllustrationViewModel.IsUgoira
                    ? new[] { await App.AppViewModel.Cache.GetAsync<IRandomAccessStream>(cacheKey) }
                    : await App.AppViewModel.Cache.GetAsync<IEnumerable<IRandomAccessStream>>(cacheKey);
                LoadingOriginalSourceTask = Task.CompletedTask;
            }
            else if (IllustrationViewModel.IsUgoira)
            {
                var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(IllustrationViewModel.Id);
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Large is { } url)
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
                            OriginalImageSources = await IOHelper.GetStreamsFromZipStreamAsync(zipStream);
                            MsIntervals = ugoiraMetadata.UgoiraMetadataInfo.Frames?.Select(x => (int)x.Delay)?.ToList();
                            break;
                        case Result<Stream>.Failure(OperationCanceledException):
                            return;
                    }

                    AdvancePhase(LoadingPhase.LoadingImage);
                    LoadingOriginalSourceTask = Task.CompletedTask;
                }
            }
            else if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                AdvancePhase(LoadingPhase.DownloadingImage);
                var ras = await imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<int>(d =>
                {
                    LoadingProgress = d;
                    AdvancePhase(LoadingPhase.DownloadingImage);
                }), ImageLoadingCancellationHandle);
                switch (ras)
                {
                    case Result<IRandomAccessStream>.Success(var s):
                        OriginalImageSources = new[] { s };
                        break;
                    default:
                        return;
                }

                AdvancePhase(LoadingPhase.LoadingImage);
                LoadingOriginalSourceTask = Task.CompletedTask;
            }

            if (OriginalImageStream is not null && !_disposed)
            {
                IllustrationViewerPageViewModel.UpdateCommandCanExecute();
                if (App.AppViewModel.AppSetting.UseFileCache)
                {
                    _ = await App.AppViewModel.Cache.TryAddAsync(cacheKey, OriginalImageStream!, TimeSpan.FromDays(1));
                }

                return;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }
    }

    public Visibility GetLoadingMaskVisibility(Task? loadingTask)
    {
        return !(loadingTask?.IsCompletedSuccessfully ?? false) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DisposeInternal()
    {
        OriginalImageStream?.Dispose();
        IllustrationViewModel.UnloadThumbnail(this, ThumbnailUrlOption.Medium);
        // if the loading task is null or hasn't been completed yet, the 
        // OriginalImageSources would be the thumbnail source, its disposal may 
        // causing the IllustrationGrid shows weird result such as an empty content
        if (LoadingCompletedSuccessfully)
        {
            //(OriginalImageSources as SoftwareBitmapSource)?.Dispose();
        }

        OriginalImageSources = null;
        LoadingOriginalSourceTask?.Dispose();
    }
}
