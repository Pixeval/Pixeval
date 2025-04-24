using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Controls;

public partial class PixivIllustrationItemViewModel : IllustrationItemViewModel
{
    private new Illustration Entry => (Illustration) base.Entry;

    public PixivIllustrationItemViewModel(Illustration illustration) : base(illustration)
    {
        var id = illustration.Id;
        UgoiraMetadataAsync = illustration.IsUgoira ? App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(id) : Task.FromResult<UgoiraMetadata>(null!);
    }

    public Task<UgoiraMetadata> UgoiraMetadataAsync { get; }

    public override async Task<object?> LoadOriginalImageAsync(Action<LoadingPhase, double> advancePhase, CancellationToken token)
    {
        return await base.LoadOriginalImageAsync(advancePhase, token);
        var metadata = null as UgoiraMetadata;
        if (Entry.IsUgoira)
            metadata = await UgoiraMetadataAsync;

        var isOriginal = App.AppViewModel.AppSettings.BrowseOriginalImage;

        // 原图动图（一张一张下）
        if (metadata?.LargeUrl is { } ugoiraUrl)
        {
            if (isOriginal)
            {
                var urls = UgoiraOriginalUrls;
                var list = new List<Stream>();
                var ratio = 1d / urls.Length;
                var startProgress = 0d;
                foreach (var url in urls)
                {
                    if (await DownloadUrlAsync(url, startProgress, ratio) is { } stream)
                        list.Add(stream);
                    else
                    {
                        list = null;
                        break;
                    }

                    startProgress += 100 * ratio;
                }

                return (list, metadata.Delays);
            }
            // 非原图动图（压缩包）
            return (await DownloadUrlAsync(ugoiraUrl), metadata.Delays);
        }
        // 静图
        if (await DownloadUrlAsync(StaticUrl(isOriginal)) is { } s)
            return s;

        return null;

        async Task<Stream?> DownloadUrlAsync(string url, double startProgress = 0, double ratio = 1)
            {
                advancePhase(LoadingPhase.CheckingCache, 0);
                if (token.IsCancellationRequested)
                    return null;
                return await CacheHelper.GetStreamFromCacheAsync(
                    url,
                    new Progress<double>(d => advancePhase(LoadingPhase.DownloadingImage, startProgress + ratio * d)),
                    cancellationToken: token);
            }
    }

    public string IllustrationLargeUrl => Entry.ThumbnailUrls.Large;

    public string MangaSingleLargeUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Large;

    public async ValueTask<string> UgoiraMediumZipUrlAsync() => (await UgoiraMetadataAsync).MediumUrl;

    public string IllustrationOriginalUrl => Entry.OriginalSingleUrl!;

    public string MangaSingleOriginalUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Original;

    public IReadOnlyList<string> MangaOriginalUrls => Entry.MangaOriginalUrls;

    public string[] UgoiraOriginalUrls => Entry.GetUgoiraOriginalUrls(UgoiraMetadataAsync.Result.Frames.Count);


    /// <summary>
    /// 单图和单图漫画的链接
    /// </summary>
    private string StaticUrl(bool original)
    {
        return (IsManga, original) switch
        {
            (true, true) => MangaSingleOriginalUrl,
            (true, false) => MangaSingleLargeUrl,
            (false, true) => IllustrationOriginalUrl,
            _ => IllustrationLargeUrl
        };
    }
}
