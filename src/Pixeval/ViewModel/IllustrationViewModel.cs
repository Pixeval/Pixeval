using System;
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


        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(_isSelected, value, this, (_, b) =>
            {
                _isSelected = b;
                OnIsSelectedChanged?.Invoke(this, this);
            });
        }

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

        public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

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

        public Task RemoveBookmarkAsync()
        {
            IsBookmarked = false;
            return App.MakoClient.RemoveBookmarkAsync(Id);
        }

        public Task PostPublicBookmarkAsync()
        {
            IsBookmarked = true;
            return App.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
        }
    }
}