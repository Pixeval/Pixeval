#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewModel.cs
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
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Options;
using Pixeval.Controls.Illustrate;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls.IllustrationView;

/// <summary>
///     A view model that communicates between the model <see cref="Illustration" /> and the view
///     <see cref="IllustrationView" />.
///     It is responsible for being the elements of the <see cref="ItemsRepeater" /> to present the thumbnail of an
///     illustration
/// </summary>
public class IllustrationViewModel(Illustration illustration) : IllustrateViewModel<Illustration>(illustration)
{
    private bool _isSelected;

    public int MangaIndex { get; set; }

    public bool IsRestricted => Illustrate.IsRestricted();

    public bool IsManga => Illustrate.IsManga();

    public BadgeMode RestrictionCaption =>
        Illustrate.RestrictLevel() switch
        {
            XRestrictLevel.R18 => BadgeMode.R18,
            XRestrictLevel.R18G => BadgeMode.R18G,
            _ => BadgeMode.R18
        };

    public string Id => Illustrate.Id.ToString();

    public int Bookmark => Illustrate.TotalBookmarks;

    public DateTimeOffset PublishDate => Illustrate.CreateDate;

    public string? OriginalSourceUrl => Illustrate.GetOriginalUrl();

    public bool IsBookmarked
    {
        get => Illustrate.IsBookmarked;
        set => SetProperty(Illustrate.IsBookmarked, value, m =>
        {
            Illustrate.IsBookmarked = m;
            OnPropertyChanged(nameof(BookmarkedColor));
        });
    }

    public SolidColorBrush BookmarkedColor => new(IsBookmarked ? Colors.Crimson : Color.FromArgb(0x80, 0, 0, 0));

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(_isSelected, value, this, (_, b) =>
        {
            _isSelected = b;
            IsSelectedChanged?.Invoke(this, this);
        });
    }

    public event EventHandler<IllustrationViewModel>? IsSelectedChanged;

    public bool IsUgoira => Illustrate.IsUgoira();

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
        if (Illustrate.PageCount <= 1)
        {
            return new[] { this };
        }

        // The API result of manga (a work with multiple illustrations) is a single Illustration object
        // that only differs from the illustrations of a single work on the MetaPages property, this property
        // contains the download urls of the manga

        return Illustrate.MetaPages!.Select(m => Illustrate with
        {
            ImageUrls = m.ImageUrls
        }).Select((p, i) => new IllustrationViewModel(p)
        {
            MangaIndex = i
        });
    }

    #region Thumbnail

    /// <summary>
    /// 缩略图图片
    /// </summary>
    public ImmutableDictionary<ThumbnailUrlOption, SoftwareBitmapSource> ThumbnailSources => ThumbnailSourcesRef.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.Value);

    /// <summary>
    /// 缩略图文件流
    /// </summary>
    public IReadOnlyDictionary<ThumbnailUrlOption, IRandomAccessStream> ThumbnailStreams => ThumbnailStreamsRef;

    private Dictionary<ThumbnailUrlOption, IRandomAccessStream> ThumbnailStreamsRef { get; } = [];

    private Dictionary<ThumbnailUrlOption, SharedRef<SoftwareBitmapSource>> ThumbnailSourcesRef { get; } = [];

    private CancellationHandle LoadingThumbnailCancellationHandle { get; } = new();

    /// <summary>
    /// 是否正在加载缩略图
    /// </summary>
    private bool LoadingThumbnail { get; set; }

    /// <summary>
    /// 当控件需要显示图片时，调用此方法加载缩略图
    /// </summary>
    /// <param name="key">使用<see cref="IDisposable"/>对象，防止复用本对象的时候，本对象持有对<paramref name="key"/>的引用，导致<paramref name="key"/>无法释放</param>
    /// <param name="thumbnailUrlOption"></param>
    /// <returns>缩略图首次加载完成则返回<see langword="true"/>，之前已加载、正在加载或加载失败则返回<see langword="false"/></returns>
    public async Task<bool> TryLoadThumbnail(IDisposable key, ThumbnailUrlOption thumbnailUrlOption)
    {
        if (ThumbnailSourcesRef.TryGetValue(thumbnailUrlOption, out var value))
        {
            _ = value.MakeShared(key);

            // 之前已加载
            return false;
        }

        if (LoadingThumbnail)
        {
            // 已有别的线程正在加载缩略图
            return false;
        }

        LoadingThumbnail = true;
        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(Illustrate.GetIllustrationThumbnailCacheKey(thumbnailUrlOption)) is { } stream)
        {
            ThumbnailStreamsRef[thumbnailUrlOption] = stream;
            ThumbnailSourcesRef[thumbnailUrlOption] = new(await stream.GetSoftwareBitmapSourceAsync(false), key);

            // 读取缓存并加载完成
            LoadingThumbnail = false;
            OnPropertyChanged(nameof(ThumbnailSources));
            return true;
        }

        if (await GetThumbnail(thumbnailUrlOption) is { } ras)
        {
            if (App.AppViewModel.AppSetting.UseFileCache)
            {
                _ = await App.AppViewModel.Cache.TryAddAsync(Illustrate.GetIllustrationThumbnailCacheKey(thumbnailUrlOption), ras, TimeSpan.FromDays(1));
            }
            ThumbnailStreamsRef[thumbnailUrlOption] = ras;
            ThumbnailSourcesRef[thumbnailUrlOption] = new(await ras.GetSoftwareBitmapSourceAsync(false), key);

            // 获取并加载完成
            LoadingThumbnail = false;
            OnPropertyChanged(nameof(ThumbnailSources));
            return true;
        }

        // 加载失败
        LoadingThumbnail = false;
        return false;
    }

    /// <summary>
    /// 当控件不显示，或者Unload时，调用此方法以尝试释放内存
    /// </summary>
    public void UnloadThumbnail(object key, ThumbnailUrlOption thumbnailUrlOption)
    {
        if (LoadingThumbnail)
        {
            LoadingThumbnailCancellationHandle.Cancel();
            LoadingThumbnail = false;
            return;
        }

        if (App.AppViewModel.AppSetting.UseFileCache)
            return;

        if (!ThumbnailSourcesRef.TryGetValue(thumbnailUrlOption, out var value))
            return;

        if (!value.TryDispose(key))
            return;

        if (ThumbnailStreamsRef.TryGetValue(thumbnailUrlOption, out var stream))
            stream?.Dispose();
        _ = ThumbnailStreamsRef.Remove(thumbnailUrlOption);
        _ = ThumbnailSourcesRef.Remove(thumbnailUrlOption);
        OnPropertyChanged(nameof(ThumbnailSources));
    }

    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<IRandomAccessStream?> GetThumbnail(ThumbnailUrlOption thumbnailUrlOptions)
    {
        if (Illustrate.GetThumbnailUrl(thumbnailUrlOptions) is { } url)
        {
            switch (await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
            {
                case Result<IRandomAccessStream>.Success(var stream):
                    return stream;
                case Result<IRandomAccessStream>.Failure(OperationCanceledException):
                    LoadingThumbnailCancellationHandle.Reset();
                    return null;
            }
        }

        return await AppContext.GetNotAvailableImageStreamAsync();
    }

    /// <summary>
    /// 强制释放所有缩略图
    /// </summary>
    private void DisposeInternal()
    {
        foreach (var (option, softwareBitmapSource) in ThumbnailSourcesRef)
        {
            softwareBitmapSource?.DisposeForce();
            ThumbnailStreamsRef[option]?.Dispose();
        }
        ThumbnailSourcesRef.Clear();
        ThumbnailStreamsRef.Clear();
    }

    public override void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    #endregion

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

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Illustrate.Title);
            if (Illustrate.IsUgoira())
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAnUgoira);
            }

            if (Illustrate.IsManga())
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAMangaFormatted.Format(Illustrate.PageCount));
            }

            return sb.ToString();
        }
    }

    public string GetBookmarkContextItemText(bool isBookmarked)
    {
        return isBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
    }

    public bool Equals(IllustrationViewModel x, IllustrationViewModel y)
    {
        return x.Illustrate.Equals(y.Illustrate);
    }

    public override bool Equals(object? obj) => obj is IllustrationViewModel viewModel && Illustrate.Equals(viewModel.Illustrate);

    public override int GetHashCode() => Illustrate.GetHashCode();
}
