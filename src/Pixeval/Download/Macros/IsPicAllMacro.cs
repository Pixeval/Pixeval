// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class IsPicAllMacro : IPredicate<IWorkViewModel>
{
    public bool IsNot { get; set; }

    public string Name => "if_pic_all";

    public bool Match(IWorkViewModel context) => context is IllustrationItemViewModel;
}
