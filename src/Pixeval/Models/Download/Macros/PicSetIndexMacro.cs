// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class PicSetIndexMacro : ITransducer<IArtworkInfo>, ILastSegment, IContextRestrictedMacro
{
    public const string NameConst = "pic_set_index";

    public string Name => NameConst;

    public string RequiredPredicateName => IsPicSetMacro.NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionPicSetIndex);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsIntegerFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = context.ImageType is ImageType.ImageSet;
        return includeToken ? MacroHelper.CreateToken(NameConst, formatter) : "";
    }
}
