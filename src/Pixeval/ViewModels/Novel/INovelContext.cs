// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Pixeval.ViewModels;

public interface INovelContext<TImage> where TImage : class
{
    NovelContent NovelContent { get; }

    Dictionary<(long, int), NovelIllustration> IllustrationLookup { get; }

    Dictionary<(long, int), TImage> IllustrationImages { get; }

    Dictionary<long, NovelImage> ImageLookup { get; }

    Dictionary<long, TImage> UploadedImages { get; }

    CancellationTokenSource LoadingCts { get; }

    void InitImages()
    {
        foreach (var illustration in NovelContent.Illustrations)
        {
            var key = (illustration.Id, illustration.Page);
            IllustrationLookup[key] = illustration;
            IllustrationImages[key] = null!;
        }

        foreach (var image in NovelContent.Images)
        {
            ImageLookup[image.NovelImageId] = image;
            UploadedImages[image.NovelImageId] = null!;
        }
    }
}
