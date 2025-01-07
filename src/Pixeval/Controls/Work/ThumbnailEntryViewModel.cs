#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ThumbnailEntryViewModel.cs
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
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
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
            ThumbnailSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>()
                .GetSourceFromMemoryCacheAsync(
                    ThumbnailUrl,
                    cancellationToken: LoadingThumbnailCancellationTokenSource.Token);
            LoadingThumbnail = false;
        }

        return true;
    }

    /// <summary>
    /// 当控件不显示，或者Unload时，调用此方法以尝试释放内存
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
        LoadingThumbnailCancellationTokenSource.TryCancelDispose();
        ThumbnailSource = null;
        DisposeOverride();
        GC.SuppressFinalize(this);
    }

    protected virtual void DisposeOverride()
    {
    }

    public override bool Equals(object? obj) => obj is ThumbnailEntryViewModel<T> viewModel && Entry.Equals(viewModel.Entry);

    public override int GetHashCode() => Entry.GetHashCode();
}
