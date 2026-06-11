// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class WorkIdMacro : ITransducer<IArtworkInfo>
{
    public string Name => "id";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionId);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsStringFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = false;
        return MacroHelper.FormatString(context.Id, formatter);
    }
}
