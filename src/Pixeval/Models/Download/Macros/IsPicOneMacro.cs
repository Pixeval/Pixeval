// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsPicOneMacro : IPredicate<IArtworkInfo>
{
    public const string NameConst = "is_pic_one";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsPicOne);

    public bool Match(IArtworkInfo context) => context.ImageType is ImageType.SingleImage;
}
