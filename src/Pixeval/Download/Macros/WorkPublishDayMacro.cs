// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class WorkPublishDayMacro : ITransducer<IArtworkInfo>
{
    public string Name => "publish_day";

    public string Substitute(IArtworkInfo context) => context.CreateDate.Day.ToString();
}
