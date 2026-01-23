// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class WorkPublishMonthMacro : ITransducer<IArtworkInfo>
{
    public string Name => "publish_month";

    public string Substitute(IArtworkInfo context) => context.CreateDate.Month.ToString();
}
