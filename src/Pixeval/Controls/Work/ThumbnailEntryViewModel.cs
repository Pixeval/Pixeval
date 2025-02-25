// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO.Caching;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public abstract partial class ThumbnailEntryViewModel<T>(T entry) : EntryViewModel<T>(entry)
    where T : class, IIdEntry
{
    public long Id => Entry.Id;

    private HashSet<int> References { get; } = [];

    protected abstract string ThumbnailUrl { get; }

    /// <summary>
    /// 缩略图图片
    /// </summary>
    [ObservableProperty]
    public partial ImageSource? ThumbnailSource { get; protected set; }

    private CancellationTokenSource LoadingThumbnailCancellationTokenSource { get; } = new();

    /// <summary>
    /// 是否正在加载缩略图
    /// </summary>
    protected bool LoadingThumbnail { get; set; }

    /// <summary>
    /// 当控件需要显示图片时，调用此方法加载缩略图
    /// </summary>
    /// <returns>缩略图首次加载完成则返回<see langword="true"/>，之前已加载、正在加载或加载失败则返回<see langword="false"/></returns>
    public virtual async ValueTask<bool> TryLoadThumbnailAsync(object key)
    {
        _ = References.Add(key.GetHashCode());
        if (ThumbnailSource is null)
        {
            LoadingThumbnail = true;
            ThumbnailSource = await CacheHelper.GetSourceFromCacheAsync(
                ThumbnailUrl,
                cancellationToken: LoadingThumbnailCancellationTokenSource.Token);

            LoadingThumbnail = false;
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
        if (LoadingThumbnail)
        {
            LoadingThumbnailCancellationTokenSource.TryCancel();
            LoadingThumbnail = false;
        }

        ThumbnailSource = null;
    }

    /// <summary>
    /// 强制释放所有缩略图
    /// </summary>
    public sealed override void Dispose()
    {
        GC.SuppressFinalize(this);
        LoadingThumbnailCancellationTokenSource.TryCancelDispose();
        ThumbnailSource = null;
        DisposeOverride();
    }

    protected virtual void DisposeOverride()
    {
    }

    public override bool Equals(object? obj) => obj is ThumbnailEntryViewModel<T> viewModel && Entry.Equals(viewModel.Entry);

    public override int GetHashCode() => Entry.GetHashCode();
}
