// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class IsR18GMacro : IPredicate<IWorkViewModel>
{
    public bool IsNot { get; set; }

    public string Name => "if_r18g";

    public bool Match(IWorkViewModel context)
    {
        return context is { IsXRestricted: true, XRestrictionCaption: BadgeMode.R18G };
    }
}
