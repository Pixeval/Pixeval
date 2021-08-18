using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Mako.Global.Enum;
using Mako.Model;
using Mako.Net;
using Mako.Util;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Options;
using Pixeval.Util;
using SoftwareBitmapSource = Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource;

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

        public async Task LoadThumbnailIfRequired()
        {
            if (ThumbnailSource is not null || LoadingThumbnail)
            {
                return;
            }
            var source = new SoftwareBitmapSource();
            LoadingThumbnail = true;
            using var ras = await GetThumbnail(ThumbnailUrlOption.Medium);
            if (ras is not null)
            {
                var decoder = await BitmapDecoder.CreateAsync(ras);
                var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await source.SetBitmapAsync(bitmap);
                ThumbnailSource = source;
            }
            LoadingThumbnail = false;
        }

        public async Task<IRandomAccessStream?> GetThumbnail(ThumbnailUrlOption thumbnailUrlOptions)
        {
            if (Illustration.GetThumbnailUrl(thumbnailUrlOptions) is { } url)
            {
                var cacheKey = Illustration.GetIllustrationCacheKey();
                if (await FileCache.Default.TryGetAsync<IRandomAccessStream>(cacheKey) is { } ras)
                {
                    Trace.WriteLine(FileCache.Default.HitCount);
                    return ras;
                }

                switch (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
                {
                    case Result<IRandomAccessStream>.Success(var stream):
                        _ = FileCache.Default.TryAddAsync(cacheKey, stream.AsStream(), TimeSpan.FromDays(1));
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
                sb.Append("这是一张动图");
            }

            if (Illustration.IsManga())
            {
                sb.AppendLine();
                sb.Append($"这是一副图集，内含{Illustration.PageCount}张图片");
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            _thumbnailSource?.Dispose();
        }
    }
}