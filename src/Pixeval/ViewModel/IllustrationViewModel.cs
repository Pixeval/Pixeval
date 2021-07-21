using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;
using Mako.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class IllustrationViewModel : ObservableObject
    {
        private readonly Illustration _illustration;

        public string Id => _illustration.Id.ToString();

        public bool IsBookmarked
        {
            get => _illustration.IsBookmarked;
            set => SetProperty(_illustration.IsBookmarked, value, m => _illustration.IsBookmarked = m);
        }

        public bool IsSelected { get; set; }

        private ImageSource? _imageSource;

        public ImageSource? ThumbnailSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public IllustrationViewModel(Illustration illustration)
        {
            _illustration = illustration;
            _ = LoadThumbnail();
        }

        public async Task LoadThumbnail()
        {
            if (_illustration.GetThumbnailUrl(ThumbnailUrlOptions.Medium) is { } url)
            {
                var ras = (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url)).GetOrThrow();
                ThumbnailSource = await ras.GetImageSourceAsync();
                return;
            }

            ThumbnailSource = UIHelper.GetImageSourceFromUriRelativeToAssetsImageFolder("image-not-available.png");
        }

        public Task PostPublicBookmarkAsync()
        {
            return App.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
        }
    }
}