// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Model;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class FileExtensionMacro : ITransducer<IArtworkInfo>, ILastSegment
{
    public const string NameConst = "ext";

    public const string NameConstToken = $".<{NameConst}>";

    public string Name => NameConst;

    public string Substitute(IArtworkInfo context)
    {
        return context.ImageType switch
        {
            ImageType.SingleImage or ImageType.ImageSet => IoHelper.GetIllustrationExtension(),
            ImageType.SingleAnimatedImage => IoHelper.GetUgoiraExtension(),
            ImageType.Other => IoHelper.GetNovelExtension(),
            _ => ""
        };
    }
}
