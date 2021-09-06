using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class IllustrationViewModel : ObservableObject, IDisposable
    {
        public Illustration Illustration { get; }

        public bool IsRestricted => Illustration.IsRestricted();

        public string RestrictionCaption => Illustration.RestrictLevel().GetMetadataOnEnumMember()!;

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

        private SoftwareBitmapSource? _thumbnailSource;

        public SoftwareBitmapSource? ThumbnailSource
        {
            get => _thumbnailSource;
            set => SetProperty(ref _thumbnailSource, value);
        }

        public CancellationHandle LoadingThumbnailCancellationHandle { get; }

        public bool LoadingThumbnail { get; private set; }

        public IllustrationViewModel(Illustration illustration)
        {
            Illustration = illustration;
            LoadingThumbnailCancellationHandle = new CancellationHandle();
        }

        public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

        public IEnumerable<IllustrationViewModel> GetMangaIllustrationViewModels()
        {
            return Illustration.GetMangaIllustrations().Select(p => new IllustrationViewModel(p));
        }

        public async Task LoadThumbnailIfRequired()
        {
            if (ThumbnailSource is not null || LoadingThumbnail)
            {
                return;
            }

            LoadingThumbnail = true;
            if (App.AppSetting.UseFileCache && await App.Cache.TryGetAsync<IRandomAccessStream>(Illustration.GetIllustrationCacheKey()) is { } stream)
            {
                ThumbnailSource = await stream.GetSoftwareBitmapSourceAsync(true);
            }
            else if (await GetThumbnail(ThumbnailUrlOption.Medium) is { } ras)
            {
                using (ras)
                {
                    await App.Cache.TryAddAsync(Illustration.GetIllustrationCacheKey(), ras!, TimeSpan.FromDays(1));
                    ThumbnailSource = await ras!.GetSoftwareBitmapSourceAsync(false);
                }
            }
            LoadingThumbnail = false;
        }

        public async Task<IRandomAccessStream?> GetThumbnail(ThumbnailUrlOption thumbnailUrlOptions)
        {
            if (Illustration.GetThumbnailUrl(thumbnailUrlOptions) is { } url)
            {
                switch (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
                {
                    case Result<IRandomAccessStream>.Success(var stream):
                        return stream;
                    case Result<IRandomAccessStream>.Failure(OperationCanceledException):
                        LoadingThumbnailCancellationHandle.Reset();
                        return null;
                    case var other:
                        return other.GetOrThrow();
                }
            }
            return await AppContext.GetAssetStreamAsync("Images/image-not-available.png");
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
                sb.Append(MiscResources.TheIllustrationIsAnUgoira);
            }

            if (Illustration.IsManga())
            {
                sb.AppendLine();
                sb.Append(MiscResources.TheIllustrationIsAMangaFormatted.Format(Illustration.PageCount));
            }

            return sb.ToString();
        }

        public void DisposeInternal()
        {
            _thumbnailSource?.Dispose();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        ~IllustrationViewModel()
        {
            Dispose();
        }
    }
}