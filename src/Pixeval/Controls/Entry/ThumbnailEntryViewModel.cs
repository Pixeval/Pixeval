#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IllustrationItemViewModel.Thumbnail.cs
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
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Controls;

public abstract partial class ThumbnailEntryViewModel<T> : EntryViewModel<T>, IBookmarkableViewModel where T : IEntry
{
    protected ThumbnailEntryViewModel(T illustrate) : base(illustrate) => InitializeCommands();

    public abstract long Id { get; }

    public abstract int Bookmark { get; }

    public abstract bool IsBookmarked { get; set; }

    public abstract Tag[] Tags { get; }

    public abstract string Title { get; }

    public abstract UserInfo User { get; }

    public abstract DateTimeOffset PublishDate { get; }

    protected abstract string ThumbnailUrl { get; }

    /// <summary>
    /// 缩略图图片
    /// </summary>
    public SoftwareBitmapSource? ThumbnailSource => ThumbnailSourceRef?.Value;

    /// <summary>
    /// 缩略图文件流
    /// </summary>
    public Stream? ThumbnailStream { get; set; }

    private SharedRef<SoftwareBitmapSource>? _thumbnailSourceRef;

    public SharedRef<SoftwareBitmapSource>? ThumbnailSourceRef
    {
        get => _thumbnailSourceRef;
        set
        {
            if (Equals(_thumbnailSourceRef, value))
                return;
            _thumbnailSourceRef = value;
            OnPropertyChanged(nameof(ThumbnailSource));
        }
    }

    private CancellationHandle LoadingThumbnailCancellationHandle { get; } = new();

    /// <summary>
    /// 是否正在加载缩略图
    /// </summary>
    protected bool LoadingThumbnail { get; set; }

    /// <summary>
    /// 当控件需要显示图片时，调用此方法加载缩略图
    /// </summary>
    /// <param name="key">使用<see cref="IDisposable"/>对象，防止复用本对象的时候，本对象持有对<paramref name="key"/>的引用，导致<paramref name="key"/>无法释放</param>
    /// <returns>缩略图首次加载完成则返回<see langword="true"/>，之前已加载、正在加载或加载失败则返回<see langword="false"/></returns>
    public virtual async Task<bool> TryLoadThumbnailAsync(IDisposable key)
    {
        if (ThumbnailSourceRef is not null)
        {
            _ = ThumbnailSourceRef.MakeShared(key);

            // 之前已加载
            return false;
        }

        if (LoadingThumbnail)
        {
            // 已有别的线程正在加载缩略图
            return false;
        }

        var cacheKey = MakoHelper.GetCacheKeyForThumbnailAsync(ThumbnailUrl);

        LoadingThumbnail = true;
        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(cacheKey) is { } stream)
        {
            ThumbnailStream = stream;
            ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await stream.GetSoftwareBitmapSourceAsync(false), key);

            // 读取缓存并加载完成
            LoadingThumbnail = false;
            OnPropertyChanged(nameof(ThumbnailSource));
            return true;
        }

        if (await GetThumbnailAsync() is { } s)
        {
            if (App.AppViewModel.AppSettings.UseFileCache)
            {
                _ = await App.AppViewModel.Cache.TryAddAsync(cacheKey, s, TimeSpan.FromDays(1));
            }
            ThumbnailStream = s;
            ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await s.GetSoftwareBitmapSourceAsync(false), key);

            // 获取并加载完成
            LoadingThumbnail = false;
            return true;
        }

        // 加载失败
        LoadingThumbnail = false;
        return false;
    }


    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<Stream?> GetThumbnailAsync()
    {
        switch (await App.AppViewModel.MakoClient.DownloadStreamAsync(ThumbnailUrl, cancellationHandle: LoadingThumbnailCancellationHandle))
        {
            case Result<Stream>.Success(var stream):
                return stream;
            case Result<Stream>.Failure(OperationCanceledException):
                LoadingThumbnailCancellationHandle.Reset();
                return null;
        }

        return AppInfo.GetNotAvailableImageStream();
    }

    /// <summary>
    /// 当控件不显示，或者Unload时，调用此方法以尝试释放内存
    /// </summary>
    public void UnloadThumbnail(object key)
    {
        if (LoadingThumbnail)
        {
            LoadingThumbnailCancellationHandle.Cancel();
            LoadingThumbnail = false;
            return;
        }

        if (App.AppViewModel.AppSettings.UseFileCache)
            return;

        if (!ThumbnailSourceRef?.TryDispose(key) ?? true)
            return;

        ThumbnailSourceRef = null;
        ThumbnailStream?.Dispose();
    }

    /// <summary>
    /// 强制释放所有缩略图
    /// </summary>
    public sealed override void Dispose()
    {
        ThumbnailSourceRef?.DisposeForce();
        ThumbnailStream?.Dispose();
        DisposeOverride();
        GC.SuppressFinalize(this);
    }

    protected virtual void DisposeOverride()
    {
    }
}

