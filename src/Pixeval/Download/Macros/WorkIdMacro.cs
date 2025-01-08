// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class WorkIdMacro : ITransducer<IWorkViewModel>
{
    public string Name => "id";

    public string Substitute(IWorkViewModel context)
    {
        return context.Id.ToString();
    }
}
