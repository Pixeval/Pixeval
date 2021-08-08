using System;
using System.Text;
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

        private ImageSource? _thumbnailSource;

        public ImageSource? ThumbnailSource
        {
            get => _thumbnailSource;
            set => SetProperty(ref _thumbnailSource, value);
        }

        public IllustrationViewModel(Illustration illustration)
        {
            Illustration = illustration;
        }

        public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

        public async Task LoadThumbnailIfRequired()
        {
            if (ThumbnailSource is not null)
            {
                return;
            }
            if (Illustration.GetThumbnailUrl(ThumbnailUrlOptions.Medium) is { } url)
            {
                var ras = (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url)).GetOrThrow();
                ThumbnailSource = await ras.GetImageSourceAsync();
                return;
            }

            ThumbnailSource = UIHelper.GetImageSourceFromUriRelativeToAssetsImageFolder("image-not-available.png");
        }

        public void UnloadThumbnail()
        {
            ThumbnailSource = null;
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

        public string GetTooltip()
        {
            var sb = new StringBuilder(Id);
            if (Illustration.IsUgoira())
            {
                sb.AppendLine();
                sb.AppendLine("这是一张动图");
            }

            if (Illustration.IsManga())
            {
                sb.AppendLine();
                sb.AppendLine($"这是一副图集，内含{Illustration.PageCount}张图片");
            }

            return sb.ToString();
        }
    }
}