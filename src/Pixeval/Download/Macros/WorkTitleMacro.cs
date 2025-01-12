// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class WorkTitleMacro : ITransducer<IWorkViewModel>
{
    public string Name => "title";

    public string Substitute(IWorkViewModel context)
    {
        return context.Title.Let(IoHelper.NormalizePathSegment);
    }
}
