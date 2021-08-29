using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Mako.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class ImageViewerPageViewModel : ObservableObject, IDisposable
    {
        public ImageViewerPageViewModel(IllustrationViewModel illustrationViewModel)
        {
            IllustrationViewModel = illustrationViewModel;
            ImageLoadingCancellationHandle = new CancellationHandle();
            _ = LoadImage();
        }

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
                OriginalImageSource = IllustrationViewModel.ThumbnailSource;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            await LoadOriginalImage();
        }

        public async Task LoadOriginalImage()
        {
            var imageClient = App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
            if (IllustrationViewModel.Illustration.IsUgoira())
            {
                var ugoiraMetadata = await App.MakoClient.GetUgoiraMetadata(IllustrationViewModel.Id);
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url)
                {
                    var task = imageClient.DownloadAsStreamAsync(url, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle);
                    LoadingOriginalSourceTask = task;
                    var zipStream = (await task).GetOrThrow();
                    OriginalImageStream = (await IOHelper.GetGifStreamResultAsync(zipStream, ugoiraMetadata)).GetOrThrow();
                    OriginalImageSource = await OriginalImageStream.GetBitmapImageSourceAsync();
                    return;
                }
            }

            if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                var task = imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle);
                LoadingOriginalSourceTask = task;
                OriginalImageStream = (await task).GetOrThrow();
                OriginalImageSource = await OriginalImageStream.GetBitmapImageSourceAsync();
                return;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }

        public Visibility GetLoadingMaskVisibility(Task? loadingTask) => !(loadingTask?.IsCompletedSuccessfully ?? false) ? Visibility.Visible : Visibility.Collapsed;

        public void Dispose()
        {
            OriginalImageStream?.Dispose();
        }
    }
}