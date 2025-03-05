using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public interface INovelContext<TImage> where TImage : class
{
    NovelContent NovelContent { get; }

    Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; }

    Dictionary<(long, int), TImage> IllustrationImages { get; }

    Dictionary<long, TImage> UploadedImages { get; }

    CancellationTokenSource LoadingCancellationTokenSource { get; }

    /// <summary>
    /// 此处默认所有图片扩展名都相同
    /// </summary>
    /// <remarks>
    /// 包含前缀点，例如".png"
    /// </remarks>
    public string? ImageExtension { get; }

    public void InitImages()
    {
        foreach (var illust in NovelContent.Illusts)
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
