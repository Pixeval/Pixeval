// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public interface INovelParserViewModel<TImage> : IDisposable
{
    NovelContent NovelContent { get; } 

    Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; }

    Dictionary<(long, int), TImage> IllustrationImages { get; }

    Dictionary<long, TImage> UploadedImages { get; }

    /// <summary>
    /// 此处默认所有图片扩展名都相同
    /// </summary>
    /// <remarks>
    /// 包含前缀点，例如".png"
    /// </remarks>
    public string? ImageExtension { get; }
}
