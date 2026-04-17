// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Mako.Model;
using Misaki;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Views.Work;

namespace Pixeval.ViewModels;

/// <summary>
/// A view model that communicates between the model <see cref="Illustration" /> and the view <see cref="WorkView" />.
/// It is responsible for being the elements of the <see cref="ListBox" /> to present the thumbnail of an illustration
/// </summary>
public partial class IllustrationItemViewModel(IArtworkInfo entry)
    : WorkEntryViewModel<IArtworkInfo>(entry), IFactory<IArtworkInfo, IllustrationItemViewModel>
{
    public virtual async Task<object?> LoadOriginalImageAsync(Action<LoadingPhase, double> advancePhase,
        CancellationToken token)
    {
        // TODO isOriginal
        var isOriginal = false;
        switch (Entry)
        {
            // 当下载图集的其中一张图片时，ImageType会为ImageSet
            case ISingleImage { ImageType: ImageType.SingleImage or ImageType.ImageSet } singleImage:
            {
                var f = isOriginal ? singleImage : Entry.Thumbnails.PickMax();
                if (f is null)
                    return null;
                var stream = await CacheHelper.GetSingleImageAsync(
                    singleImage, 
                    f,
                    new Progress<double>(d => advancePhase(LoadingPhase.DownloadingImage, d)),
                    token);
                return stream;
            }
            case ISingleAnimatedImage { ImageType: ImageType.SingleAnimatedImage } singleAnimatedImage:
            {
                var f = isOriginal
                    ? singleAnimatedImage
                    : (await singleAnimatedImage.AnimatedThumbnails.ApplyAsync(t => t
                        .TryPreloadListAsync(singleAnimatedImage))).PickMax();
                if (f is null)
                    return null;
                switch (f.PreferredAnimatedImageType)
                {
                    case SingleAnimatedImageType.MultiFiles:
                    {
                        var list = await CacheHelper.GetAnimatedImageSeparatedAsync(
                            singleAnimatedImage,
                            f,
                            new Progress<double>(d => advancePhase(LoadingPhase.DownloadingImage, d)),
                            token);
                        var arr1 = new Stream[list.Count];
                        var arr2 = new int[list.Count];
                        for (var i = 0; i < list.Count; ++i)
                            (arr1[i], arr2[i]) = list[i];
                        return (arr1, arr2);
                    }
                    case SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile:
                    {
                        var stream = await CacheHelper.GetSingleImageAsync(
                            singleAnimatedImage,
                            f,
                            new Progress<double>(d => advancePhase(LoadingPhase.DownloadingImage, d)),
                            token);
                        if (f.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile)
                            return await IoHelper.SplitAnimatedImageStreamAsync(stream);
                        await f.ZipImageDelays!.TryPreloadListAsync(singleAnimatedImage);
                        var zip = await Streams.ReadZipAsync(stream, true);
                        return (zip, f.ZipImageDelays);
                    }
                }

                break;
            }
        }

        return null;
    }

    /// <summary>
    /// 当调用<see cref="GetMangaIllustrationViewModels"/>后，此属性会被赋值为当前<see cref="IllustrationItemViewModel"/>在Manga中的索引
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
        if (!IsPicSet || Entry is not IImageSet set)
        {
            // 保证里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            return [CreateInstance(Entry)];
        }

        return ((IEnumerable<IArtworkInfo>) set.Pages).Select(CreateInstance);
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
