using System;
using System.Threading;
using System.Threading.Tasks;
using Mako.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class ImageViewerPageViewModel : ObservableObject
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

        private bool _loading;

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        private ImageSource? _originalImageSource;

        public ImageSource? OriginalImageSource
        {
            get => _originalImageSource;
            set => SetProperty(ref _originalImageSource, value);
        }

        public IllustrationViewModel IllustrationViewModel { get; }

        public CancellationHandle ImageLoadingCancellationHandle { get; }

        private async Task LoadImage()
        {
            _ = IllustrationViewModel.LoadThumbnailIfRequired().ContinueWith(_ =>
            {
                App.Window.DispatcherQueue.TryEnqueue(() => OriginalImageSource = IllustrationViewModel.ThumbnailSource);
            });
            await LoadOriginalImage();
        }

        public async Task LoadOriginalImage()
        {
            if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                Loading = true;
                OriginalImageSource = await (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                        .DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationHandle))
                    .GetOrThrow()
                    .GetImageSourceAsync();
                Loading = false;
                return;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }
    }
}