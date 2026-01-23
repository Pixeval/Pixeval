using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Pixeval.ViewModels;

public interface INovelContext<TImage> where TImage : class
{
    NovelContent NovelContent { get; }

    Dictionary<(long, int), NovelIllustration> IllustrationLookup { get; }

    Dictionary<(long, int), TImage> IllustrationImages { get; }

    Dictionary<long, TImage> UploadedImages { get; }

    CancellationTokenSource LoadingCts { get; }

    /// <summary>
    /// 此处默认所有图片扩展名都相同
    /// </summary>
    /// <remarks>
    /// 包含前缀点，例如".png"
    /// </remarks>
    string? ImageExtension { get; }

    void InitImages()
    {
        foreach (var illust in NovelContent.Illustrations)
        {
            var key = (illust.Id, illust.Page);
            IllustrationLookup[key] = illust;
            IllustrationImages[key] = null!;
        }

        foreach (var image in NovelContent.Images)
        {
            UploadedImages[image.NovelImageId] = null!;
        }
    }
}
