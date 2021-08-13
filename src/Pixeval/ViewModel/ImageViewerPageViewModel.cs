using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Mako.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pixeval.Util;

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

        private async Task LoadImage()
        {
            _ = IllustrationViewModel.LoadThumbnailIfRequired().ContinueWith(_ =>
            {
                App.Window.DispatcherQueue.TryEnqueue(() => IllustrationViewModel.OriginalImageSource = IllustrationViewModel.ThumbnailSource);
            });
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
                    IllustrationViewModel.OriginalImageSource = await OriginalImageStream.GetImageSourceAsync();
                    return;
                }
            }

            if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                var task = imageClient.DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle);
                LoadingOriginalSourceTask = task;
                OriginalImageStream = (await task).GetOrThrow();
                IllustrationViewModel.OriginalImageSource = await OriginalImageStream.GetImageSourceAsync();
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