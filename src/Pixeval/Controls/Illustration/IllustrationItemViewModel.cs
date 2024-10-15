#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewModel.cs
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

    public Task<UgoiraMetadataResponse> UgoiraMetadata { get; }

    public string IllustrationLargeUrl => Entry.ThumbnailUrls.Large;

    public string MangaSingleLargeUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Large;

    public async ValueTask<string> UgoiraMediumZipUrlAsync() => (await UgoiraMetadata).MediumUrl;

    public string IllustrationOriginalUrl => Entry.OriginalSingleUrl!;

    public string MangaSingleOriginalUrl => Entry.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Original;

    public IReadOnlyList<string> MangaOriginalUrls => Entry.MangaOriginalUrls;

    public List<string> UgoiraOriginalUrls => Entry.GetUgoiraOriginalUrls(UgoiraMetadata.Result.FrameCount);

    public string DimensionText => $"{Entry.Width} x {Entry.Height}";

    public async ValueTask<List<string>> UgoiraOriginalUrlsAsync() => Entry.GetUgoiraOriginalUrls((await UgoiraMetadata).FrameCount);

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
