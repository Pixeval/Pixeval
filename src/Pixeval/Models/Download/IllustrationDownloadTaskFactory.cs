// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Misaki;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IArtworkInfo, IDownloadTaskGroup, int>
{
    public IDownloadTaskGroup Create(IArtworkInfo context, string rawPath, int setIndex = -1) =>
        Create(new ParserContext(context), rawPath, setIndex);

    public IDownloadTaskGroup Create(ParserContext parserContext, string rawPath, int setIndex = -1)
    {
        parserContext = SelectPage(parserContext, setIndex);
        var context = parserContext.ArtworkInfo;
        var path = IoHelper.NormalizePath(DownloadPathMacroParser.Reduce(rawPath, parserContext));
        var workSubscriptionId = parserContext.WorkSubscription?.HistoryEntryId;

        IDownloadTaskGroup task = context switch
        {
            ISingleImage { ImageType: ImageType.SingleImage } singleImage => new SingleImageDownloadTaskGroup(
                singleImage, path, workSubscriptionId),
            ISingleImage { ImageType: ImageType.ImageSet, SetIndex: > -1 } singleImage =>
                new SingleImageDownloadTaskGroup(singleImage, path, workSubscriptionId),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.MultiFiles
            } singleAnimatedImage => new UgoiraDownloadTaskGroup(singleAnimatedImage, path, workSubscriptionId),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleFile
                or SingleAnimatedImageType.SingleZipFile
            } singleAnimatedImage => new SingleAnimatedImageDownloadTaskGroup(singleAnimatedImage, path, workSubscriptionId),
            IImageSet { ImageType: ImageType.ImageSet } imageSet => new MangaDownloadTaskGroup(imageSet, path, workSubscriptionId),
            _ => throw new NotSupportedException()
        };

        return task;
    }

    private static ParserContext SelectPage(ParserContext parserContext, int setIndex) =>
        setIndex >= 0 && parserContext.ArtworkInfo is IImageSet imageSet
            ? parserContext with { ArtworkInfo = imageSet.Pages[setIndex] }
            : parserContext;
}
