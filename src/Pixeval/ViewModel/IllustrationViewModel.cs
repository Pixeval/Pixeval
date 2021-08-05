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
        public Illustration Illustration { get; }

        public string Id => Illustration.Id.ToString();

        public int Bookmark => Illustration.TotalBookmarks;

        public DateTimeOffset PublishDate => Illustration.CreateDate;

        public string? OriginalSourceUrl => Illustration.GetOriginalUrl();

        public bool IsBookmarked
        {
            get => Illustration.IsBookmarked;
            set => SetProperty(Illustration.IsBookmarked, value, m => Illustration.IsBookmarked = m);
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
            Illustration = illustration;
            _ = LoadThumbnail();
        }

        public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

        public async Task LoadThumbnail()
        {
            if (Illustration.GetThumbnailUrl(ThumbnailUrlOptions.Medium) is { } url)
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