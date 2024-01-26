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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// A view model that communicates between the model <see cref="Illustration" /> and the view <see cref="IllustrationView" />.
/// It is responsible for being the elements of the <see cref="ItemsRepeater" /> to present the thumbnail of an illustration
/// </summary>
public partial class IllustrationItemViewModel : IllustrateViewModel<Illustration>
{
    public IllustrationItemViewModel(Illustration illustration) : base(illustration) => InitializeCommands();

    /// <summary>
    /// 当调用<see cref="GetMangaIllustrationViewModels"/>后，此属性会被赋值为当前<see cref="IllustrationItemViewModel"/>在Manga中的索引
    /// </summary>
    public int MangaIndex { get; set; } = -1;

    public bool IsRestricted => Illustrate.XRestrict is not XRestrict.Ordinary;

    public bool IsManga => Illustrate.PageCount > 1;

    public BadgeMode RestrictionCaption =>
        Illustrate.XRestrict switch
        {
            XRestrict.R18 => BadgeMode.R18,
            XRestrict.R18G => BadgeMode.R18G,
            _ => BadgeMode.R18
        };

    public long Id => Illustrate.Id;

    public int Bookmark => Illustrate.TotalBookmarks;

    public DateTimeOffset PublishDate => Illustrate.CreateDate;

    /// <summary>
    /// <see cref="IsUgoira"/>为<see langword="true"/>时，此属性不会抛异常<br/>
    /// 同一个漫画图片的格式会不会不同？
    /// </summary>
    public async Task<(UgoiraMetadataResponse Metadata, string Url)> GetUgoiraOriginalUrlAsync()
    {
        var metadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(Id);
        return (metadata, metadata.UgoiraMetadataInfo.ZipUrls.Large);
    }

    /// <summary>
    /// <see cref="IsUgoira"/>为<see langword="false"/>时，此属性不为<see langword="null"/>
    /// </summary>
    public string? OriginalStaticUrl => IsManga
        ? Illustrate.MetaPages[MangaIndex is -1 ? 0 : MangaIndex].ImageUrls.Original
        : Illustrate.MetaSinglePage.OriginalImageUrl;

    public async Task<string> GetOriginalSourceUrlAsync() => IsUgoira
        ? (await GetUgoiraOriginalUrlAsync()).Url
        : OriginalStaticUrl;

    public bool IsBookmarked
    {
        get => Illustrate.IsBookmarked;
        set => SetProperty(Illustrate.IsBookmarked, value, m => Illustrate.IsBookmarked = m);
    }

    [MemberNotNullWhen(false, nameof(OriginalStaticUrl))]
    public bool IsUgoira => Illustrate.Type is "ugoira";

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Illustrate.Title);
            if (IsUgoira)
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAnUgoira);
            }

            if (IsManga)
            {
                _ = sb.AppendLine()
                    .Append(MiscResources.TheIllustrationIsAMangaFormatted.Format(Illustrate.PageCount));
            }

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
        if (Illustrate.PageCount <= 1)
        {
            // 保证里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            return [new(Illustrate)];
        }

        // The API result of manga (a work with multiple illustrations) is a single Illustration object
        // that only differs from the illustrations of a single work on the MetaPages property, this property
        // contains the download urls of the manga

        return Illustrate.MetaPages.Select(m => Illustrate with { ImageUrls = m.ImageUrls })
            .Select((p, i) => new IllustrationItemViewModel(p) { MangaIndex = i });
    }

    public IEnumerable<string> GetMangaImageUrls()
    {
        return Illustrate.MetaPages.Select(m => m.ImageUrls.Original!);
    }

    public bool Equals(IllustrationItemViewModel x, IllustrationItemViewModel y)
    {
        return x.Illustrate.Equals(y.Illustrate);
    }

    public override bool Equals(object? obj) => obj is IllustrationItemViewModel viewModel && Illustrate.Equals(viewModel.Illustrate);

    public override int GetHashCode() => Illustrate.GetHashCode();
}
