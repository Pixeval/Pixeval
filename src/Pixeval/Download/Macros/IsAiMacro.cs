// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class IsAiMacro : IPredicate<IWorkViewModel>
{
    public bool IsNot { get; set; }

    public string Name => "if_ai";

    public bool Match(IWorkViewModel context)
    {
        return context.IsAiGenerated;
    }
}
