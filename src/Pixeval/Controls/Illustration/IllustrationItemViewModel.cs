// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.Pages.IllustrationViewer;
using System;
using System.Linq;
using System.Threading;
using Misaki;
using WinUI3Utilities;
using Pixeval.Util.IO.Caching;
using System.IO;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

/// <summary>
/// A view model that communicates between the model <see cref="Illustration" /> and the view <see cref="WorkView" />.
/// It is responsible for being the elements of the <see cref="ItemsRepeater" /> to present the thumbnail of an illustration
/// </summary>
public partial class IllustrationItemViewModel : WorkEntryViewModel<IArtworkInfo>, IFactory<IArtworkInfo, IllustrationItemViewModel>
{
    public IllustrationItemViewModel(IArtworkInfo entry) : base(entry)
    {
        MangaSaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;
        MangaSaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;
        var isManga = IsManga;
        MangaSaveCommand.CanExecuteRequested += (_, e) => e.CanExecute = isManga;
        MangaSaveAsCommand.CanExecuteRequested += (_, e) => e.CanExecute = isManga;
    }

    public virtual async Task<object?> LoadOriginalImageAsync(Action<LoadingPhase, double> advancePhase,
        CancellationToken token)
    {
        var isOriginal = App.AppViewModel.AppSettings.BrowseOriginalImage;
        switch (Entry)
        {
            case ISingleImage { ImageType: ImageType.SingleImage } singleImage:
            {
                var f = isOriginal ? singleImage : Entry.Thumbnails.PickMaxFrame();
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
                        .TryPreloadListAsync(singleAnimatedImage))).PickMaxFrame();
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
    public int MangaIndex => Entry is ISingleImage single
        ? single.SetIndex
        : -1;

    public bool IsManga => Entry.ImageType is ImageType.ImageSet;

    public bool IsUgoira => Entry.ImageType is ImageType.SingleAnimatedImage;

    public string? SizeText => Entry is IImageFrame frame ? $"{frame.Width} x {frame.Height}" : null;

    public string Tooltip
    {
        get
        {
            var sb = new StringBuilder(Entry.Title);
            if (IsUgoira)
                _ = sb.AppendLine()
                    .Append(EntryItemResources.TheIllustrationIsAnUgoira);
            else if (IsManga && Entry is IImageSet set)
                _ = sb.AppendLine()
                    .Append(EntryItemResources.TheIllustrationIsAMangaFormatted.Format(set.PageCount));

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
        if (!IsManga || Entry is not IImageSet set)
        {
            // 保证里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            return [CreateInstance(Entry)];
        }

        return ((IEnumerable<IArtworkInfo>) set.Pages).Select(CreateInstance);
    }

    public override Uri AppUri => Entry.AppUri;

    public override Uri WebsiteUri => Entry.WebsiteUri;

    public static IllustrationItemViewModel CreateInstance(IArtworkInfo entry) =>
        entry.Platform switch
        {
            IPlatformInfo.Pixiv => new PixivIllustrationItemViewModel((Illustration) entry),
            _ => new IllustrationItemViewModel(entry)
        };

    protected override Task<bool> SetBookmarkAsync(bool privately = false, IEnumerable<string>? tags = null) => ThrowHelper.NotSupported<Task<bool>>();
}
