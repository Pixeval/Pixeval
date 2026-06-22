// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class FileExtensionMacro : ITransducer<IArtworkInfo>, ILastSegment
{
    public const string NameConst = "ext";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionExt);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsStringFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = true;
        return $"<{NameConst}:{formatter}>";
    }
}
