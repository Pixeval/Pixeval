#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Attributes;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.UserControls;

/// <summary>
///     A view model that communicates between the model <see cref="Illustration" /> and the view
///     <see cref="IllustrationGrid" />.
///     It is responsible for being the elements of the <see cref="AdaptiveGridView" /> to present the thumbnail of an
///     illustration
/// </summary>
public partial class IllustrationViewModel : ObservableObject, IDisposable
{
    private bool _isSelected;

    [ObservableProperty]
    private SoftwareBitmapSource? _thumbnailSource;

    public IllustrationViewModel(Illustration illustration)
    {
        Illustration = illustration;
        LoadingThumbnailCancellationHandle = new CancellationHandle();
    }

    public Illustration Illustration { get; }

    public int MangaIndex { get; set; }

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

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(_isSelected, value, this, (_, b) =>
        {
            _isSelected = b;
            OnIsSelectedChanged?.Invoke(this, this);
        });
    }

    public CancellationHandle LoadingThumbnailCancellationHandle { get; }

    public bool LoadingThumbnail { get; private set; }

    public bool IsUgoira => Illustration.IsUgoira();

    public void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    public event EventHandler<IllustrationViewModel>? OnIsSelectedChanged;

    /// <summary>
    ///     An illustration may contains multiple works and such illustrations are named "manga".
    ///     This method attempts to get the works and wrap into <see cref="IllustrationViewModel" />
    /// </summary>
    /// <returns>
    ///     A collection of a single <see cref="IllustrationViewModel" />, if the illustration is not
    ///     a manga, that is to say, contains only a single work.
    ///     A collection of multiple <see cref="IllustrationViewModel" />, if the illustration is a manga
    ///     that consist of multiple works
    /// </returns>
    public IEnumerable<IllustrationViewModel> GetMangaIllustrationViewModels()
    {
        if (Illustration.PageCount <= 1)
        {
            return new[] { this };
        }

        // The API result of manga (a work with multiple illustrations) is a single Illustration object
        // that only differs from the illustrations of a single work on the MetaPages property, this property
        // contains the download urls of the manga

        return Illustration.MetaPages!.Select(m => Illustration with
        {
            ImageUrls = m.ImageUrls
        }).Select((p, i) => new IllustrationViewModel(p)
        {
            MangaIndex = i
        });
    }

    public async Task<bool> LoadThumbnailIfRequired()
    {
        if (ThumbnailSource is not null || LoadingThumbnail)
        {
            return false;
        }

        LoadingThumbnail = true;
        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(Illustration.GetIllustrationThumbnailCacheKey()) is { } stream)
        {
            ThumbnailSource = await stream.GetSoftwareBitmapSourceAsync(true);
            LoadingThumbnail = false;
            return true;
        }

        if (await GetThumbnail(ThumbnailUrlOption.Medium) is { } ras)
        {
            using (ras)
            {
                await App.AppViewModel.Cache.TryAddAsync(Illustration.GetIllustrationThumbnailCacheKey(), ras, TimeSpan.FromDays(1));
                ThumbnailSource = await ras.GetSoftwareBitmapSourceAsync(false);
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
            switch (await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
            {
                case Result<IRandomAccessStream>.Success (var stream):
                    return stream;
                case Result<IRandomAccessStream>.Failure (OperationCanceledException):
                    LoadingThumbnailCancellationHandle.Reset();
                    return null;
            }
        }

        return await AppContext.GetNotAvailableImageStreamAsync();
    }

    public Task SwitchBookmarkStateAsync()
    {
        return IsBookmarked ? RemoveBookmarkAsync() : PostPublicBookmarkAsync();
    }

    public Task RemoveBookmarkAsync()
    {
        IsBookmarked = false;
        return App.AppViewModel.MakoClient.RemoveBookmarkAsync(Id);
    }

    public Task PostPublicBookmarkAsync()
    {
        IsBookmarked = true;
        return App.AppViewModel.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
    }

    public string Tooltip => GetTooltip();

    public string GetTooltip()
    {
        var sb = new StringBuilder(Illustration.Title);
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

    public string GetBookmarkContextItemText(bool isBookmarked)
    {
        return isBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
    }

    public void DisposeInternal()
    {
        _thumbnailSource?.Dispose();
    }

    ~IllustrationViewModel()
    {
        Dispose();
    }
}