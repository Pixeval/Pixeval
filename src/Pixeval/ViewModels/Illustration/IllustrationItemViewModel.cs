// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Mako.Model;
using Misaki;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Views.Work;

namespace Pixeval.ViewModels;

/// <summary>
/// A view model that communicates between the model <see cref="Illustration" /> and the view <see cref="WorkView" />.
/// It is responsible for being the elements of the <see cref="ListBox" /> to present the thumbnail of an illustration
/// </summary>
public partial class IllustrationItemViewModel(IArtworkInfo entry)
    : WorkEntryViewModel<IArtworkInfo>(entry), IFactory<IArtworkInfo, IllustrationItemViewModel>
{
    /// <summary>
    /// 在<see cref="IImageSet.Pages"/>中，此属性会被赋值为当前<see cref="IllustrationItemViewModel"/>在Manga中的索引
    /// </summary>
    public int SetIndex => Entry is ISingleImage single
        ? single.SetIndex
        : -1;

    public bool IsPicSet => Entry.ImageType is ImageType.ImageSet;

    public bool IsPicGif => Entry.ImageType is ImageType.SingleAnimatedImage;

    public bool IsPicOne => Entry.ImageType is ImageType.SingleImage;

    public double AspectRatio => Entry is { Width: > 0, Height: > 0 }
        ? (double) Entry.Width / Entry.Height
        : 1;

    public string? SizeText =>
        Entry is IImageFrame { Width: > 0, Height: > 0 } frame ? $"{frame.Width} x {frame.Height}" : null;

    public int PageCount => Entry is IImageSet set ? set.PageCount : 1;

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Entry.Title);
            if (IsPicGif)
                _ = sb.AppendLine()
                    .Append(I18NManager.GetResource(EntryItemResources.TheIllustrationIsAnUgoira));
            else if (IsPicSet && Entry is IImageSet set)
                _ = sb.AppendLine()
                    .Append(string.Format(I18NManager.GetResource(EntryItemResources.TheIllustrationIsAMangaFormatted), set.PageCount));

            return sb.ToString();
        }
    }

    public override Uri AppUri => Entry.AppUri;

    public override Uri WebsiteUri => Entry.WebsiteUri;

    public static IllustrationItemViewModel CreateInstance(IArtworkInfo entry) => new(entry);

    protected override Task<bool> SetBookmarkAsync(bool favorite, bool privately = false, IEnumerable<string>? tags = null)
        => MakoHelper.SetIllustrationBookmarkAsync((Illustration) Entry, favorite, privately, tags);
}

[LocalizationMetadata]
public enum LoadingPhase
{
    [LocalizedResource(ImageViewerPageResources.CheckingCache)]
    CheckingCache,

    [LocalizedResource(ImageViewerPageResources.LoadingFromCache)]
    LoadingFromCache,

    [LocalizedResource(ImageViewerPageResources.MergingUgoiraFrames)]
    MergingUgoiraFrames,

    [LocalizedResource(ImageViewerPageResources.DownloadingImageFormatted)]
    DownloadingImage,

    [LocalizedResource(ImageViewerPageResources.LoadingImage)]
    LoadingImage,
}
