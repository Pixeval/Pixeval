// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels;

public abstract partial class ThumbnailEntryViewModel<T>(T entry) : EntryViewModel<T>(entry), IDisposable
    where T : class, IIdentityInfo
{
    public string Id => Entry.Id;

    private HashSet<int> References { get; } = [];

    protected abstract string ThumbnailUrl { get; }

    /// <summary>
    /// 缩略图图片
    /// </summary>
    [ObservableProperty]
    public partial Bitmap? Thumbnail { get; protected set; }

    private readonly CancellationTokenSource _loadingThumbnailCts = new();

    /// <summary>
    /// 是否正在加载缩略图
    /// </summary>
    protected bool ThumbnailLoaded { get; set; }

    /// <summary>
    /// 当控件需要显示图片时，调用此方法加载缩略图
    /// </summary>
    /// <returns>缩略图首次加载完成则返回<see langword="true"/>，之前已加载、正在加载或加载失败则返回<see langword="false"/></returns>
    public virtual async ValueTask<bool> TryLoadThumbnailAsync(object key)
    {
        _ = References.Add(key.GetHashCode());
        if (Thumbnail is null)
        {
            Thumbnail = await CacheHelper.GetBitmapFromCacheAsync(
                ThumbnailUrl,
                cancellationToken: _loadingThumbnailCts.Token);

            ThumbnailLoaded = true;
        }

        return true;
    }

    /// <summary>
    /// 当控件Unload时，调用此方法以尝试释放内存
    /// </summary>
    public void UnloadThumbnail(object key)
    {
        _ = References.Remove(key.GetHashCode());
        if (References.Count is not 0)
            return;
        if (ThumbnailLoaded)
        {
            _loadingThumbnailCts.Cancel();
            ThumbnailLoaded = true;
        }

        Thumbnail = null;
    }

    /// <summary>
    /// 强制释放所有缩略图
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ThumbnailLoaded = false;
        _loadingThumbnailCts.Cancel();
        Thumbnail?.Dispose();
        Thumbnail = null;
        DisposeOverride();
    }

    protected virtual void DisposeOverride()
    {
    }

    public override bool Equals(object? obj) => obj is ThumbnailEntryViewModel<T> viewModel && Entry.Equals(viewModel.Entry);

    public override int GetHashCode() => Entry.GetHashCode();
}
