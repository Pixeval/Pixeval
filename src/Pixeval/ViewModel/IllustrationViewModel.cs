using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    /// <summary>
    /// A view model that communicates between the model <see cref="Illustration"/> and the view <see cref="IllustrationGrid"/>.
    /// It is responsible for being the elements of the <see cref="AdaptiveGridView"/> to present the thumbnail of an illustration
    /// </summary>
    public class IllustrationViewModel : ObservableObject, IDisposable
    {
        public Illustration Illustration { get; }

        public bool IsRestricted => Illustration.IsRestricted();

        public bool IsManga => Illustration.IsManga();

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

        public bool IsUgoira => Illustration.IsUgoira();

        public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

        /// <summary>
        /// An illustration may contains multiple works and such illustrations are named "manga".
        /// This method attempts to get the works and wrap into <see cref="IllustrationViewModel"/>
        /// </summary>
        /// <returns>
        /// A collection of a single <see cref="IllustrationViewModel"/>, if the illustration is not
        /// a manga, that is to say, contains only a single work.
        /// A collection of multiple <see cref="IllustrationViewModel"/>, if the illustration is a manga
        /// that consist of multiple works
        /// </returns>
        public IEnumerable<IllustrationViewModel> GetMangaIllustrationViewModels()
        {
            if (Illustration.PageCount <= 1)
            {
                return new[] {this};
            }

            // The API result of manga (a work with multiple illustrations) is a single Illustration object
            // that only differs from the illustrations of a single work on the MetaPages property, this property
            // contains the download urls of the manga

            return Illustration.MetaPages!.Select(m => Illustration with
            {
                ImageUrls = m.ImageUrls
            }).Select(p => new IllustrationViewModel(p));
        }

        public async Task<bool> LoadThumbnailIfRequired()
        {
            if (ThumbnailSource is not null || LoadingThumbnail)
            {
                return false;
            }

            LoadingThumbnail = true;
            if (App.AppSetting.UseFileCache && await App.Cache.TryGetAsync<IRandomAccessStream>(Illustration.GetIllustrationThumbnailCacheKey()) is { } stream)
            {
                ThumbnailSource = await stream.GetSoftwareBitmapSourceAsync(true);
                LoadingThumbnail = false;
                return true;
            }

            if (await GetThumbnail(ThumbnailUrlOption.Medium) is { } ras)
            {
                using (ras)
                {
                    await App.Cache.TryAddAsync(Illustration.GetIllustrationThumbnailCacheKey(), ras!, TimeSpan.FromDays(1));
                    ThumbnailSource = await ras!.GetSoftwareBitmapSourceAsync(false);
                }

                LoadingThumbnail = false;
                return true;
            }

            LoadingThumbnail = false;
            return false;
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