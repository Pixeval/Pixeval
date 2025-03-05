// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// A view model that communicates between the model <see cref="Illustration" /> and the view <see cref="WorkView" />.
/// It is responsible for being the elements of the <see cref="ItemsRepeater" /> to present the thumbnail of an illustration
/// </summary>
public partial class IllustrationItemViewModel : WorkEntryViewModel<Illustration>, IFactory<Illustration, IllustrationItemViewModel>
{
    public static IllustrationItemViewModel CreateInstance(Illustration entry) => new(entry);

    public IllustrationItemViewModel(Illustration illustration) : base(illustration)
    {
        MangaSaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;
        MangaSaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;
        var isManga = IsManga;
        MangaSaveCommand.CanExecuteRequested += (_, e) => e.CanExecute = isManga;
        MangaSaveAsCommand.CanExecuteRequested += (_, e) => e.CanExecute = isManga;
        var id = illustration.Id;
        UgoiraMetadata = illustration.IsUgoira ? App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(id) : Task.FromResult<UgoiraMetadataResponse>(null!);
    }

    /// <summary>
    /// 当调用<see cref="GetMangaIllustrationViewModels"/>后，此属性会被赋值为当前<see cref="IllustrationItemViewModel"/>在Manga中的索引
    /// </summary>
    public int MangaIndex { get; set; } = -1;

    public bool IsManga => Entry.IsManga;

    public bool IsUgoira => Entry.IsUgoira;

    public int Width => Entry.Width;

    public int Height => Entry.Height;

    public double AspectRatio => (double) Width / Height;

    public Task<UgoiraMetadataResponse> UgoiraMetadata { get; }

    public string IllustrationLargeUrl => Entry.ThumbnailUrls.Large;

    public string MangaSingleLargeUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Large;

    public async ValueTask<string> UgoiraMediumZipUrlAsync() => (await UgoiraMetadata).MediumUrl;

    public string IllustrationOriginalUrl => Entry.OriginalSingleUrl!;

    public string MangaSingleOriginalUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Original;

    public IReadOnlyList<string> MangaOriginalUrls => Entry.MangaOriginalUrls;

    public List<string> UgoiraOriginalUrls => Entry.GetUgoiraOriginalUrls(UgoiraMetadata.Result.FrameCount);

    public string SizeText => $"{Entry.Width} x {Entry.Height}";

    /// <summary>
    /// 单图和单图漫画的链接
    /// </summary>
    public string StaticUrl(bool original)
    {
        return (IsManga, original) switch
        {
            (true, true) => MangaSingleOriginalUrl,
            (true, false) => MangaSingleLargeUrl,
            (false, true) => IllustrationOriginalUrl,
            _ => IllustrationLargeUrl
        };
    }

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Title);
            if (IsUgoira)
                _ = sb.AppendLine()
                    .Append(EntryItemResources.TheIllustrationIsAnUgoira);
            else if (IsManga)
                _ = sb.AppendLine()
                    .Append(EntryItemResources.TheIllustrationIsAMangaFormatted.Format(Entry.PageCount));

            return sb.ToString();
        }
    }

    /// <summary>
    /// An illustration may contain multiple works and such illustrations are named "manga".
    /// This method attempts to get the works and wrap into <see cref="IllustrationItemViewModel" />
    /// </summary>
    /// <returns>
    /// A collection of a single <see cref="IllustrationItemViewModel" />, if the illustration is not
    /// a manga, that is to say, contains only a single work.
    /// A collection of multiple <see cref="IllustrationItemViewModel" />, if the illustration is a manga
    /// that consist of multiple works
    /// </returns>
    public IEnumerable<IllustrationItemViewModel> GetMangaIllustrationViewModels()
    {
        if (Entry.PageCount <= 1)
        {
            // 保证里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            return [new(Entry)];
        }

        // The API result of manga (a work with multiple illustrations) is a single Illustration object
        // that only differs from the illustrations of a single work on the MetaPages property, this property
        // contains the download urls of the manga

        return Entry.MetaPages.Select(m => Entry with
        {
            MetaSinglePage = new() { OriginalImageUrl = m.ImageUrls.Original },
            ThumbnailUrls = m.ImageUrls
        }).Select((p, i) => new IllustrationItemViewModel(p) { MangaIndex = i });
    }
}
