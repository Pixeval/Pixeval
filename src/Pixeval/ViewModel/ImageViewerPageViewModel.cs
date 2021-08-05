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
            ImageLoadingCancellationTokenSource = new CancellationTokenSource();
            Loading = true;
        }

        private bool _loading ;

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        private double _loadingProgress;

        public double LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
        }

        public IllustrationViewModel IllustrationViewModel { get; }

        public CancellationTokenSource ImageLoadingCancellationTokenSource { get; }

        public async Task<ImageSource> LoadOriginalImage()
        {
            if (IllustrationViewModel.OriginalSourceUrl is { } src)
            {
                var image = await (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                        .DownloadAsIRandomAccessStreamAsync(src, new Progress<double>(d => LoadingProgress = d), ImageLoadingCancellationTokenSource.Token))
                    .GetOrThrow()
                    .GetImageSourceAsync();
                Loading = false;
                return image;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }
    }
}