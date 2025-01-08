// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class WorkPublishYearMacro : ITransducer<IWorkViewModel>
{
    public string Name => "publish_year";

    public string Substitute(IWorkViewModel context)
    {
        return context.PublishDate.Year.ToString();
    }
}
