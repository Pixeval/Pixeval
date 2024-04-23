#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/DocumentViewerViewModel.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Database;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Controls;

public class DocumentViewerViewModel(NovelContent novelContent) : ObservableObject, IDisposable
{
    public Action<int>? JumpToPageRequested;

    public NovelContent NovelContent { get; } = novelContent;

    public Dictionary<long, NovelIllustInfo> IllustrationLookup { get; } = [];

    public Dictionary<(long, int), SoftwareBitmapSource> IllustrationImages { get; } = [];

    public Dictionary<long, SoftwareBitmapSource> UploadedImages { get; } = [];

    public ObservableCollection<List<Paragraph>> Pages { get; } = [];

    public async Task LoadContentAsync()
    {
        _ = LoadImagesAsync();

        var index = 0;
        var length = NovelContent.Text.Length;
        var parser = new PixivNovelRtfParser();
        Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
        await Task.Yield();
        while (index < length)
        {
            if (LoadingCancellationHandle.IsCancelled)
                break;
            Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
        }
    }

    public async Task LoadImagesAsync()
    {
        foreach (var illust in NovelContent.Illusts)
        {
            IllustrationLookup[illust.Id] = illust;
            IllustrationImages[(illust.Id, illust.Page)] = null!;
        }

        foreach (var image in NovelContent.Images)
            UploadedImages[image.NovelImageId] = null!;

        foreach (var illust in NovelContent.Illusts)
        {
            if (LoadingCancellationHandle.IsCancelled)
                break;
            var key = (illust.Id, illust.Page);
            IllustrationImages[key] = await LoadThumbnailAsync(illust.Illust.Images.Medium);
            OnPropertyChanged(nameof(IllustrationImages) + key.GetHashCode());
        }

        foreach (var image in NovelContent.Images)
        {
            if (LoadingCancellationHandle.IsCancelled)
                break;
            UploadedImages[image.NovelImageId] = await LoadThumbnailAsync(image.Urls.X1200);
            OnPropertyChanged(nameof(UploadedImages) + image.NovelImageId);
        }
    }

    private async Task<SoftwareBitmapSource> LoadThumbnailAsync(string url)
    {
        var cacheKey = MakoHelper.GetCacheKeyForThumbnailAsync(url);

        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(cacheKey) is { } stream)
        {
            return await stream.GetSoftwareBitmapSourceAsync(false);
        }

        var s = await GetThumbnailAsync(url);
        if (App.AppViewModel.AppSettings.UseFileCache)
            await App.AppViewModel.Cache.AddAsync(cacheKey, s, TimeSpan.FromDays(1));
        return await s.GetSoftwareBitmapSourceAsync(false);
    }

    private CancellationHandle LoadingCancellationHandle { get; } = new();

    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<Stream> GetThumbnailAsync(string url)
    {
        return await App.AppViewModel.MakoClient.DownloadStreamAsync(url, cancellationHandle: LoadingCancellationHandle) is
            Result<Stream>.Success(var stream)
            ? stream
            : AppInfo.GetNotAvailableImageStream();
    }

    public static async Task<DocumentViewerViewModel> CreateAsync(long novelId)
    {
        var novelContent = await App.AppViewModel.MakoClient.GetNovelContentAsync(novelId);
        var vm = new DocumentViewerViewModel(novelContent);
        _ = vm.LoadContentAsync();
        AddHistory();

        return vm;

        void AddHistory()
        {
            using var scope = App.AppViewModel.AppServicesScope;
            var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
            _ = manager.Delete(x => x.Id == novelId && x.Type == SimpleWorkType.Novel);
            manager.Insert(new BrowseHistoryEntry { Id = novelId, Type = SimpleWorkType.Novel });
        }
    }

    public void Dispose()
    {
        LoadingCancellationHandle.Cancel();
        foreach (var (_, value) in IllustrationImages)
            value?.Dispose();
        foreach (var (_, value) in UploadedImages)
            value?.Dispose();
    }
}
