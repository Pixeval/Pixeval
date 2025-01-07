// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

/// <summary>
/// 包含R18G
/// </summary>
[MetaPathMacro<IWorkViewModel>]
public class IsR18Macro : IPredicate<IWorkViewModel>
{
    public bool IsNot { get; set; }

    public string Name => "if_r18";

    public bool Match(IWorkViewModel context)
    {
        return context.IsXRestricted;
    }
}
