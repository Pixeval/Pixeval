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
using Pixeval.Util.UI;

#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval.ViewModel
{
    public class ImageViewerPageViewModel : ObservableObject, IDisposable
    {
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
            if (await App.Cache.TryGetAsync<IRandomAccessStream>(cacheKey) is { } stream)
            {
                OriginalImageStream = stream;
                OriginalImageSource = IllustrationViewModel.Illustration.IsUgoira()
                    ? await OriginalImageStream.GetBitmapImageSourceAsync()
                    : await stream.GetSoftwareBitmapSourceAsync(true);
                LoadingOriginalSourceTask = Task.CompletedTask;
                return;
            }
            if (IllustrationViewModel.Illustration.IsUgoira())
            {
                var ugoiraMetadata = await App.MakoClient.GetUgoiraMetadata(IllustrationViewModel.Id);
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url)
                {
                    var task = imageClient.DownloadAsStreamAsync(url, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle);
                    LoadingOriginalSourceTask = task;
                    switch (await task)
                    {
                        case Result<Stream>.Success(var zipStream):
                            OriginalImageStream = (await IOHelper.GetGifStreamResultAsync(zipStream, ugoiraMetadata)).GetOrThrow();
                            break;
                        case Result<Stream>.Failure(OperationCanceledException):
                            return;
                    }
                    
                    OriginalImageSource = await OriginalImageStream!.GetBitmapImageSourceAsync();
                }
            }

            if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                var task = imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle);
                LoadingOriginalSourceTask = task;

                switch (await task)
                {
                    case Result<IRandomAccessStream>.Success (var s):
                        OriginalImageStream = s;
                        break;
                    case Result<IRandomAccessStream>.Failure (OperationCanceledException):
                        return;
                }
                OriginalImageSource = await OriginalImageStream!.GetSoftwareBitmapSourceAsync(false);
            }

            if (OriginalImageStream is not null && !_disposed)
            {
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
            if (IllustrationViewerPageViewModel.FirstImageViewerPageViewModel.IsBookmarked)
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
#if DEBUG
            Trace.WriteLine($"Disposing ImageViewerPageViewModel for {IllustrationViewModel.Id}");
#endif
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