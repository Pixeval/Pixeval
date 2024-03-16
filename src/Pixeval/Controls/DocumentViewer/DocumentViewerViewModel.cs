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
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Controls;

public partial class DocumentViewerViewModel : ObservableObject, IDisposable
{
    public NovelContent NovelContent { get; }

    public Dictionary<long, SoftwareBitmapSource> IllustrationImages { get; } = [];

    public Dictionary<long, SoftwareBitmapSource> UploadedImages { get; } = [];

    public ObservableCollection<Paragraph> Paragraphs { get; } = [];

    public Paragraph? CurrentParagraph => CurrentPage > Paragraphs.Count ? Paragraphs[CurrentPage] : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentParagraph))]
    private int _currentPage;

    public DocumentViewerViewModel(NovelContent novelContent)
    {
        NovelContent = novelContent;
    }

    public async Task LoadContentAsync()
    {
        _ = LoadImagesAsync();

        Paragraphs.AddRange(PixivNovelParser.Parse(NovelContent.Text, this));
    }

    public async Task LoadImagesAsync()
    {
        foreach (var illust in NovelContent.Illusts)
            IllustrationImages[illust.Id] = null!;

        foreach (var image in NovelContent.Images)
            UploadedImages[image.NovelImageId] = null!;

        foreach (var illust in NovelContent.Illusts)
        {
            IllustrationImages[illust.Id] = await LoadThumbnailAsync(illust.Illust.Images.Medium);
            OnPropertyChanged(nameof(IllustrationImages) + illust.Id);
        }

        foreach (var image in NovelContent.Images)
        {
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
        switch (await App.AppViewModel.MakoClient.DownloadStreamAsync(url, cancellationHandle: LoadingCancellationHandle))
        {
            case Result<Stream>.Success(var stream):
                return stream;
            case Result<Stream>.Failure(OperationCanceledException):
                LoadingCancellationHandle.Reset();
                break;
        }

        return AppInfo.GetNotAvailableImageStream();
    }

    public static async Task<DocumentViewerViewModel> CreateAsync(long novelId)
    {
        var novelContent = await App.AppViewModel.MakoClient.GetNovelContentAsync(novelId);
        var vm = new DocumentViewerViewModel(novelContent);
        _ = vm.LoadContentAsync();

        return vm;
    }

    public void Dispose()
    {
    }
}
