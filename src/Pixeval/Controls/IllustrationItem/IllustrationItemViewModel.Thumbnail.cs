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
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls;

public partial class IllustrationItemViewModel
{
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
             OnPropertyChanging(nameof(ThumbnailSource));
             _thumbnailSourceRef = value;
             OnPropertyChanged(nameof(ThumbnailSource));
         }
     }

    private CancellationHandle LoadingThumbnailCancellationHandle { get; } = new();

    /// <summary>
    /// 是否正在加载缩略图
    /// </summary>
    private bool LoadingThumbnail { get; set; }

    /// <summary>
    /// 当控件需要显示图片时，调用此方法加载缩略图
    /// </summary>
    /// <param name="key">使用<see cref="IDisposable"/>对象，防止复用本对象的时候，本对象持有对<paramref name="key"/>的引用，导致<paramref name="key"/>无法释放</param>
    /// <returns>缩略图首次加载完成则返回<see langword="true"/>，之前已加载、正在加载或加载失败则返回<see langword="false"/></returns>
    public async Task<bool> TryLoadThumbnail(IDisposable key)
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

        LoadingThumbnail = true;
        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(await this.GetIllustrationThumbnailCacheKeyAsync()) is { } stream)
        {
            ThumbnailStream = stream;
            ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await stream.GetSoftwareBitmapSourceAsync(false), key);

            // 读取缓存并加载完成
            LoadingThumbnail = false;
            OnPropertyChanged(nameof(ThumbnailSource));
            return true;
        }

        if (await GetThumbnailAsync() is { } ras)
        {
            if (App.AppViewModel.AppSetting.UseFileCache)
            {
                _ = await App.AppViewModel.Cache.TryAddAsync(await this.GetIllustrationThumbnailCacheKeyAsync(), ras, TimeSpan.FromDays(1));
            }
            ThumbnailStream = ras;
            ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await ras.GetSoftwareBitmapSourceAsync(false), key);

            // 获取并加载完成
            LoadingThumbnail = false;
            return true;
        }

        // 加载失败
        LoadingThumbnail = false;
        return false;
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

        if (App.AppViewModel.AppSetting.UseFileCache)
            return;

        if (!ThumbnailSourceRef?.TryDispose(key) ?? true)
            return;

        ThumbnailSourceRef = null;
        ThumbnailStream?.Dispose();
    }

    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<Stream?> GetThumbnailAsync(ThumbnailUrlOption thumbnailUrlOptions = ThumbnailUrlOption.Medium)
    {
        if (Illustrate.GetThumbnailUrl(thumbnailUrlOptions) is { } url)
        {
            switch (await App.AppViewModel.MakoClient.DownloadStreamAsync(url, cancellationHandle: LoadingThumbnailCancellationHandle))
            {
                case Result<Stream>.Success(var stream):
                    return stream;
                case Result<Stream>.Failure(OperationCanceledException):
                    LoadingThumbnailCancellationHandle.Reset();
                    return null;
            }
        }

        return AppContext.GetNotAvailableImageStream();
    }

    /// <summary>
    /// 强制释放所有缩略图
    /// </summary>
    private void DisposeInternal()
    {
        ThumbnailSourceRef?.DisposeForce();
        ThumbnailStream?.Dispose();
    }

    public override void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }
}
