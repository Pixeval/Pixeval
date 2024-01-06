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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls.IllustrationView;

public partial class IllustrationItemViewModel
{
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
}
