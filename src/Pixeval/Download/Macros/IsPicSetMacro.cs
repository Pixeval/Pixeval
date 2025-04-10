// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class IsPicSetMacro : IPredicate<IArtworkInfo>
{
    public bool IsNot { get; set; }

    public const string NameConst = "if_pic_set";

    public string Name => NameConst;

    public bool Match(IArtworkInfo context) => context.ImageType is ImageType.ImageSet;
}
