// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class WorkTitleMacro : ITransducer<IArtworkInfo>
{
    public string Name => "title";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionTitle);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsStringFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = false;
        return MacroHelper.FormatString(context.Title, formatter);
    }
}
