using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.ViewModel
{
    public class ImageViewerPageViewModel : ObservableObject, IDisposable
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


        public ImageViewerPageViewModel(IllustrationViewerPageViewModel illustrationViewerPageViewModel, IllustrationViewModel illustrationViewModel)
        {
            IllustrationViewerPageViewModel = illustrationViewerPageViewModel;
            IllustrationViewModel = illustrationViewModel;
            ImageLoadingCancellationHandle = new CancellationHandle();
            _ = LoadImage();
        }

        private bool _disposed;

        private double _loadingProgress;

        public double LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
        }

        private string? _loadingText;

        public string? LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public IRandomAccessStream? OriginalImageStream { get; private set; }

        private TaskNotifier? _loadingOriginalSourceTask;

        public Task? LoadingOriginalSourceTask
        {
            get => _loadingOriginalSourceTask!;
            set => SetPropertyAndNotifyOnCompletion(ref _loadingOriginalSourceTask!, value);
        }

        public IllustrationViewModel IllustrationViewModel { get; }

        public CancellationHandle ImageLoadingCancellationHandle { get; }

        /// <summary>
        /// The view model of the <see cref="IllustrationViewerPage"/> that hosts the owner <see cref="ImageViewerPage"/>
        /// of this <see cref="ImageViewerPageViewModel"/>
        /// </summary>
        public IllustrationViewerPageViewModel IllustrationViewerPageViewModel { get; }

        private ImageSource? _originalImageSource;

        public ImageSource? OriginalImageSource
        {
            get => _originalImageSource;
            set => SetProperty(ref _originalImageSource, value);
        }

        private void AdvancePhase(LoadingPhase phase)
        {
            LoadingText = phase.GetLocalizedResource() switch
            {
                {FormatKey: LoadingPhase} attr => attr.GetLocalizedResourceContent()?.Format((int) LoadingProgress),
                var attr => attr?.GetLocalizedResourceContent()
            };
        }

        private async Task LoadImage()
        {
            _ = IllustrationViewModel.LoadThumbnailIfRequired().ContinueWith(_ =>
            {
                OriginalImageSource ??= IllustrationViewModel.ThumbnailSource;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            await LoadOriginalImage();
        }

        public async Task LoadOriginalImage()
        {
            var imageClient = App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
            var cacheKey = IllustrationViewModel.Illustration.GetIllustrationOriginalImageCacheKey();
            AdvancePhase(LoadingPhase.CheckingCache);
            if (await App.Cache.TryGetAsync<IRandomAccessStream>(cacheKey) is { } stream)
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
                var ugoiraMetadata = await App.MakoClient.GetUgoiraMetadata(IllustrationViewModel.Id);
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url)
                {
                    AdvancePhase(LoadingPhase.DownloadingGifZip);
                    switch (await imageClient.DownloadAsStreamAsync(url, new Progress<double>(d =>
                    {
                        LoadingProgress = d;
                        AdvancePhase(LoadingPhase.DownloadingGifZip);
                    }), ImageLoadingCancellationHandle))
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
                switch (await imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d =>
                {
                    LoadingProgress = d;
                    AdvancePhase(LoadingPhase.DownloadingImage);
                }), ImageLoadingCancellationHandle))
                {
                    case Result<IRandomAccessStream>.Success (var s):
                        OriginalImageStream = s;
                        break;
                    case Result<IRandomAccessStream>.Failure (OperationCanceledException):
                        // TODO add load failed image
                        return;
                }

                AdvancePhase(LoadingPhase.LoadingImage);
                OriginalImageSource = await OriginalImageStream!.EncodeSoftwareBitmapSourceAsync(false);
                LoadingOriginalSourceTask = Task.CompletedTask;
            }

            if (OriginalImageStream is not null && !_disposed)
            {
                IllustrationViewerPageViewModel.UpdateCommandCanExecute();
                await App.Cache.TryAddAsync(cacheKey, OriginalImageStream!, TimeSpan.FromDays(1));
                return;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }

        /// <summary>
        /// We use the <see cref="IllustrationViewerPageViewModel"/> to remove and add bookmark
        /// because the manga have multiple works and those works aside of this one cannot receive
        /// the bookmark notification if we use <see cref="IllustrationViewModel"/> 
        /// </summary>
        public void SwitchBookmarkState()
        {
            if (IllustrationViewerPageViewModel.FirstIllustrationViewModel.IsBookmarked)
            {
                IllustrationViewerPageViewModel.RemoveBookmarkAsync();
            }
            else
            {
                IllustrationViewerPageViewModel.PostPublicBookmarkAsync();
            }
        }

        public Visibility GetLoadingMaskVisibility(Task? loadingTask) => !(loadingTask?.IsCompletedSuccessfully ?? false) ? Visibility.Visible : Visibility.Collapsed;

        private void DisposeInternal()
        {
            OriginalImageStream?.Dispose();
            // Remarks:
            // if the loading task is null or hasn't been completed yet, the 
            // OriginalImageSource would be the thumbnail source, its disposal may 
            // causing the IllustrationGrid shows weird result such as an empty content
            if (LoadingOriginalSourceTask?.IsCompletedSuccessfully is true)
            {
                (OriginalImageSource as SoftwareBitmapSource)?.Dispose();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        ~ImageViewerPageViewModel()
        {
            Dispose();
        }
    }
}