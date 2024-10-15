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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Documents;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using System.Text;
using System.Threading;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Controls;

public class DocumentViewerViewModel(NovelContent novelContent) : ObservableObject, INovelParserViewModel<ImageSource>, INovelParserViewModel<Stream>
{
    /// <summary>
    /// 需要从外部Invoke
    /// </summary>
    public Action<int>? JumpToPageRequested;

    public NovelContent NovelContent { get; } = novelContent;

    public int TotalImagesCount { get; } = novelContent.Images.Length + novelContent.Illusts.Length;

    public string? ImageExtension { get; set; }

    /// <summary>
    /// 所有图片的URL
    /// </summary>
    public string[] AllUrls { get; } = novelContent.Images.Select(x => x.ThumbnailUrl).Concat(novelContent.Illusts.Select(x => x.ThumbnailUrl)).ToArray();

    public string[] AllTokens { get; } = novelContent.Images.Select(x => x.NovelImageId.ToString()).Concat(novelContent.Illusts.Select(x => $"{x.Id}-{x.Page}")).ToArray();

    public Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; } = [];

    Dictionary<(long, int), Stream> INovelParserViewModel<Stream>.IllustrationImages => IllustrationStreams;

    Dictionary<long, Stream> INovelParserViewModel<Stream>.UploadedImages => UploadedStreams;

    public Dictionary<(long, int), ImageSource> IllustrationImages { get; } = [];

    public Dictionary<long, ImageSource> UploadedImages { get; } = [];

    public Dictionary<(long, int), Stream> IllustrationStreams { get; } = [];

    public Dictionary<long, Stream> UploadedStreams { get; } = [];

    public ObservableCollection<List<Paragraph>> Pages { get; } = [];

    public async Task LoadRtfContentAsync()
    {
        var index = 0;
        var length = NovelContent.Text.Length;
        var parser = new PixivNovelRtfParser();
        Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
        await Task.Yield();
        while (index < length)
        {
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
            Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
        }
    }

    public StringBuilder LoadMdContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        var sb = new StringBuilder();
        for (var i = 0; index < length; ++i)
        {
            var parser = new PixivNovelMdParser(sb, i);
            _ = parser.Parse(NovelContent.Text, ref index, this);
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
        }

        return sb;
    }

    public StringBuilder LoadHtmlContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        var sb = new StringBuilder();
        for (var i = 0; index < length; ++i)
        {
            var parser = new PixivNovelHtmlParser(sb, i);
            _ = parser.Parse(NovelContent.Text, ref index, this);
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
        }

        return sb;
    }

    public Document LoadPdfContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        PixivNovelPdfParser.Init();

        return
            Document.Create(t =>
                t.Page(p =>
                {
                    p.MarginHorizontal(90);
                    p.MarginVertical(72);
                    p.DefaultTextStyle(new TextStyle().LineHeight(2));
                    p.Content().Column(c =>
                    {
                        for (var i = 0; index < length; ++i)
                        {
                            var parser = new PixivNovelPdfParser(c, i);
                            _ = parser.Parse(NovelContent.Text, ref index, this);
                            if (LoadingCancellationTokenSource.IsCancellationRequested)
                                break;
                        }
                    });
                }));
    }

    public void InitImages()
    {
        foreach (var illust in NovelContent.Illusts)
        {
            var key = (illust.Id, illust.Page);
            IllustrationLookup[key] = illust;
            IllustrationImages[key] = null!;
            IllustrationStreams[key] = null!;
        }

        foreach (var image in NovelContent.Images)
        {
            UploadedImages[image.NovelImageId] = null!;
            UploadedStreams[image.NovelImageId] = null!;
        }
    }

    public async Task LoadImagesAsync()
    {
        InitImages();

        foreach (var illust in NovelContent.Illusts)
        {
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
            var key = (illust.Id, illust.Page);
            var temp = IllustrationStreams[key] = await GetThumbnailAsync(illust.ThumbnailUrl);
            IllustrationImages[key] = await temp.GetBitmapImageAsync(false, url: illust.ThumbnailUrl);
            OnPropertyChanged(nameof(IllustrationImages) + key.GetHashCode());
        }

        foreach (var image in NovelContent.Images)
        {
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
            var temp = UploadedStreams[image.NovelImageId] = await LoadThumbnailAsync(image.ThumbnailUrl);
            UploadedImages[image.NovelImageId] = await temp.GetBitmapImageAsync(false, url: image.ThumbnailUrl);
            OnPropertyChanged(nameof(UploadedImages) + image.NovelImageId);
        }
    }

    public (long Id, IEnumerable<string> Tags)? GetIdTags(int index)
    {
        if (index < NovelContent.Images.Length)
            return null;
        var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
        return (illust.Id, illust.Illust.Tags.Select(t => t.Tag));
    }

    public Stream? TryGetStream(int index)
    {
        if (index < NovelContent.Images.Length)
            return UploadedStreams.GetValueOrDefault(NovelContent.Images[index].NovelImageId);
        var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
        return IllustrationStreams.GetValueOrDefault((illust.Id, illust.Page));
    }

    public void SetStream(int index, Stream? stream)
    {
        if (index < NovelContent.Images.Length)
        {
            UploadedStreams[NovelContent.Images[index].NovelImageId] = TryGetNotAvailableImageStream(stream);
        }
        else
        {
            var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
            IllustrationStreams[(illust.Id, illust.Page)] = TryGetNotAvailableImageStream(stream);
        }
    }

    public Stream TryGetNotAvailableImageStream(Stream? result) => result ?? AppInfo.GetImageNotAvailableStream();

    private async Task<Stream> LoadThumbnailAsync(string url)
    {
        var cacheKey = MakoHelper.GetThumbnailCacheKey(url);

        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(cacheKey) is { } stream)
        {
            return stream;
        }

        var s = await GetThumbnailAsync(url);
        if (App.AppViewModel.AppSettings.UseFileCache)
            await App.AppViewModel.Cache.AddAsync(cacheKey, s, TimeSpan.FromDays(1));
        return s;
    }

    private CancellationTokenSource LoadingCancellationTokenSource { get; } = new();

    /// <summary>
    /// 直接获取对应缩略图
    /// </summary>
    public async Task<Stream> GetThumbnailAsync(string url)
    {
        return await App.AppViewModel.MakoClient.DownloadMemoryStreamAsync(url, cancellationToken: LoadingCancellationTokenSource.Token) is
            Result<Stream>.Success(var stream)
            ? stream
            : AppInfo.GetImageNotAvailableStream();
    }

    public static async Task<DocumentViewerViewModel> CreateAsync(NovelItemViewModel novelItem, Action<Task> callback)
    {
        var novelContent = await novelItem.GetNovelContentAsync();
        var vm = new DocumentViewerViewModel(novelContent);
        var task1 = vm.LoadImagesAsync();
        var task2 = vm.LoadRtfContentAsync();
        _ = Task.WhenAll(task1, task2).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        BrowseHistoryPersistentManager.AddHistory(novelItem.Entry);
        return vm;
    }

    public INovelParserViewModel<Stream> Clone()
    {
        var vm = new DocumentViewerViewModel(NovelContent);
        foreach (var (key, value) in IllustrationLookup)
            vm.IllustrationLookup[key] = value;
        foreach (var (key, value) in IllustrationStreams)
            vm.IllustrationStreams[key] = value;
        foreach (var (key, value) in UploadedStreams)
            vm.UploadedStreams[key] = value;
        return vm;
    }

    public void Dispose()
    {
        LoadingCancellationTokenSource.Cancel();
        LoadingCancellationTokenSource.Dispose();
        IllustrationImages.Clear();
        UploadedImages.Clear();
        foreach (var (_, value) in IllustrationStreams)
            value.Dispose();
        IllustrationStreams.Clear();
        foreach (var (_, value) in UploadedStreams)
            value.Dispose();
        UploadedStreams.Clear();
        IllustrationLookup.Clear();
        GC.SuppressFinalize(this);
    }
}
