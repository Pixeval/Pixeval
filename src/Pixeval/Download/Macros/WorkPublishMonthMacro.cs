// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class WorkPublishMonthMacro : ITransducer<IWorkViewModel>
{
    public string Name => "publish_month";

    public string Substitute(IWorkViewModel context)
    {
        return context.CreateDate.Month.ToString();
    }
}
