// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class WorkPublishTimeMacro : ITransducer<IArtworkInfo>
{
    public string Name => "publish_time";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionPublishTime);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsDateTimeOffsetFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = false;
        return MacroHelper.FormatDateTimeOffset(context.CreateDate, formatter ?? "yyyy-M-d");
    }
}
