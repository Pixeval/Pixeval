// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class IsPicGifMacro : IPredicate<IArtworkInfo>
{
    public bool IsNot { get; set; }

    public string Name => "if_pic_gif";

    public bool Match(IArtworkInfo context) => context.ImageType is ImageType.SingleAnimatedImage;
}
