// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class PicSetIndexMacro : ITransducer<IArtworkInfo>, ILastSegment
{
    public const string NameConst = "pic_set_index";

    public const string NameConstToken = $"<{NameConst}>";

    public string Name => NameConst;

    public string Substitute(IArtworkInfo context) => context.ImageType is ImageType.ImageSet ? NameConstToken : "";
}
