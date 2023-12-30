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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls.IllustrationView;

/// <summary>
///     A view model that communicates between the model <see cref="Illustration" /> and the view
///     <see cref="IllustrationView" />.
///     It is responsible for being the elements of the <see cref="ItemsRepeater" /> to present the thumbnail of an
///     illustration
/// </summary>
public class IllustrationItemViewModel(Illustration illustration) : IllustrateViewModel<Illustration>(illustration)
{
    private bool _isSelected;

    /// <summary>
    /// 当调用<see cref="GetMangaIllustrationViewModels"/>后，此属性会被赋值为当前<see cref="IllustrationItemViewModel"/>在Manga中的索引
    /// </summary>
    public int MangaIndex { get; set; } = -1;

    public bool IsRestricted => Illustrate.XRestrict is not XRestrict.Ordinary;

    public bool IsManga => Illustrate.PageCount > 1;

    public BadgeMode RestrictionCaption =>
        Illustrate.XRestrict switch
        {
            XRestrict.R18 => BadgeMode.R18,
            XRestrict.R18G => BadgeMode.R18G,
            _ => BadgeMode.R18
        };

    public long Id => Illustrate.Id;

    public int Bookmark => Illustrate.TotalBookmarks;

    public DateTimeOffset PublishDate => Illustrate.CreateDate;

    /// <summary>
    /// <see cref="IsUgoira"/>为<see langword="true"/>时，此属性不会抛异常<br/>
    /// 同一个漫画图片的格式会不会不同？
    /// </summary>
    public async Task<(UgoiraMetadataResponse Metadata, string Url)> GetUgoiraOriginalUrlAsync()
    {
        var metadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(Id);
        return (metadata, metadata.UgoiraMetadataInfo.ZipUrls.Large);
    }

    /// <summary>
    /// <see cref="IsUgoira"/>为<see langword="false"/>时，此属性不为<see langword="null"/>
    /// </summary>
    public string? OriginalStaticUrl => IsManga
        ? Illustrate.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Original
        : Illustrate.MetaSinglePage.OriginalImageUrl;

    public async Task<string> GetOriginalSourceUrlAsync() => IsUgoira
        ? (await GetUgoiraOriginalUrlAsync()).Url
        : OriginalStaticUrl;

    public bool IsBookmarked
    {
        get => Illustrate.IsBookmarked;
        set => SetProperty(Illustrate.IsBookmarked, value, m =>
        {
            Illustrate.IsBookmarked = m;
            OnPropertyChanged(nameof(BookmarkedColor));
        });
    }

    public SolidColorBrush BookmarkedColor =>
        new(IsBookmarked ? Colors.Crimson : Color.FromArgb(0x80, 0, 0, 0));

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(_isSelected, value, this, (_, b) =>
        {
            _isSelected = b;
            IsSelectedChanged?.Invoke(this, this);
        });
    }

    [MemberNotNullWhen(false, nameof(OriginalStaticUrl))]
    public bool IsUgoira => Illustrate.Type is "ugoira";

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Illustrate.Title);
            if (IsUgoira)
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAnUgoira);
            }

            if (IsManga)
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAMangaFormatted.Format(Illustrate.PageCount));
            }

            return sb.ToString();
        }
    }

    public event EventHandler<IllustrationItemViewModel>? IsSelectedChanged;

    /// <summary>
    ///     An illustration may contains multiple works and such illustrations are named "manga".
    ///     This method attempts to get the works and wrap into <see cref="IllustrationItemViewModel" />
    /// </summary>
    /// <returns>
    ///     A collection of a single <see cref="IllustrationItemViewModel" />, if the illustration is not
    ///     a manga, that is to say, contains only a single work.
    ///     A collection of multiple <see cref="IllustrationItemViewModel" />, if the illustration is a manga
    ///     that consist of multiple works
    /// </returns>
    public IEnumerable<IllustrationItemViewModel> GetMangaIllustrationViewModels()
    {
        if (Illustrate.PageCount <= 1)
        {
            // 保证里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            return [new(Illustrate)];
        }

        // The API result of manga (a work with multiple illustrations) is a single Illustration object
        // that only differs from the illustrations of a single work on the MetaPages property, this property
        // contains the download urls of the manga

        return Illustrate.MetaPages.Select(m => Illustrate with { ImageUrls = m.ImageUrls })
            .Select((p, i) => new IllustrationItemViewModel(p) { MangaIndex = i });
    }

    public IEnumerable<string> GetMangaImageUrls()
    {
        return Illustrate.MetaPages.Select(m => m.ImageUrls.Original!);
    }

    public Task ToggleBookmarkStateAsync()
    {
        return IsBookmarked ? RemoveBookmarkAsync() : PostPublicBookmarkAsync();

        Task RemoveBookmarkAsync()
        {
            IsBookmarked = false;
            return App.AppViewModel.MakoClient.RemoveBookmarkAsync(Id);
        }

        Task PostPublicBookmarkAsync()
        {
            IsBookmarked = true;
            return App.AppViewModel.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
        }
    }

    public bool Equals(IllustrationItemViewModel x, IllustrationItemViewModel y)
    {
        return x.Illustrate.Equals(y.Illustrate);
    }

    public override bool Equals(object? obj) => obj is IllustrationItemViewModel viewModel && Illustrate.Equals(viewModel.Illustrate);

    public override int GetHashCode() => Illustrate.GetHashCode();

    #region Thumbnail

    /// <summary>
    /// 缩略图图片
    /// </summary>
    public ImmutableDictionary<ThumbnailUrlOption, SoftwareBitmapSource> ThumbnailSources => ThumbnailSourcesRef.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.Value);

    /// <summary>
    /// 缩略图文件流
    /// </summary>
    public IReadOnlyDictionary<ThumbnailUrlOption, IRandomAccessStream> ThumbnailStreams => ThumbnailStreamsRef;

    private ConcurrentDictionary<ThumbnailUrlOption, IRandomAccessStream> ThumbnailStreamsRef { get; } = [];

    private ConcurrentDictionary<ThumbnailUrlOption, SharedRef<SoftwareBitmapSource>> ThumbnailSourcesRef { get; } = [];

    private CancellationHandle LoadingThumbnailCancellationHandle { get; } = new CancellationHandle();

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
        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(await this.GetIllustrationThumbnailCacheKeyAsync(thumbnailUrlOption)) is { } stream)
        {
            ThumbnailStreamsRef[thumbnailUrlOption] = stream;
            ThumbnailSourcesRef[thumbnailUrlOption] = new SharedRef<SoftwareBitmapSource>(await stream.GetSoftwareBitmapSourceAsync(false), key);

            // 读取缓存并加载完成
            LoadingThumbnail = false;
            OnPropertyChanged(nameof(ThumbnailSources));
            return true;
        }

        if (await GetThumbnail(thumbnailUrlOption) is { } ras)
        {
            if (App.AppViewModel.AppSetting.UseFileCache)
            {
                _ = await App.AppViewModel.Cache.TryAddAsync(await this.GetIllustrationThumbnailCacheKeyAsync(thumbnailUrlOption), ras, TimeSpan.FromDays(1));
            }
            ThumbnailStreamsRef[thumbnailUrlOption] = ras;
            ThumbnailSourcesRef[thumbnailUrlOption] = new SharedRef<SoftwareBitmapSource>(await ras.GetSoftwareBitmapSourceAsync(false), key);

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

        if (!ThumbnailSourcesRef.TryRemove(thumbnailUrlOption, out var value) || !value.TryDispose(key))
            return;

        if (ThumbnailStreamsRef.TryRemove(thumbnailUrlOption, out var stream))
            stream?.Dispose();

        OnPropertyChanged(nameof(ThumbnailSources));
    }

    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<IRandomAccessStream?> GetThumbnail(ThumbnailUrlOption thumbnailUrlOptions)
    {
        if (Illustrate.GetThumbnailUrl(thumbnailUrlOptions) is { } url)
        {
            switch (await App.AppViewModel.MakoClient.DownloadRandomAccessStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
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

    #region Helpers

    public async Task SaveAsync(IRandomAccessStream? intrinsic = null)
    {
        var path = App.AppViewModel.AppSetting.DefaultDownloadPathMacro;
        await SaveUtilityAsync(path, intrinsic);
    }

    public async Task<bool> SaveAsAsync(Window window, IRandomAccessStream? intrinsic = null)
    {
        var folder = await window.OpenFolderPickerAsync();
        if (folder is null)
            return false;
        await SaveUtilityAsync(Path.Combine(folder.Path, GetName()), intrinsic);
        return true;

        string GetName()
        {
            string? name;
            if (IsUgoira)
                name = Id + IoHelper.GetUgoiraExtension();
            else if (MangaIndex is -1)
                name = Id + this.GetStaticImageFormat();
            else
                name = $"{Id}_{MangaIndex}";
            return name;
        }
    }

    /// <summary>
    /// <see cref="IllustrationDownloadTaskFactory"/>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="intrinsic"></param>
    /// <returns></returns>
    private async Task SaveUtilityAsync(string path, IRandomAccessStream? intrinsic = null)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
        if (intrinsic is null)
        {
            var task = await factory.CreateAsync(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
        }
        else
        {
            var task = await factory.TryCreateIntrinsicAsync(this, intrinsic, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
        }
    }

    #endregion
}
