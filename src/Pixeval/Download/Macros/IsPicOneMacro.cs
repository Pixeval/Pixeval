// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class IsPicOneMacro : IPredicate<IWorkViewModel>
{
    public bool IsNot { get; set; }

    public const string NameConst = "if_pic_one";

    public string Name => NameConst;

    public bool Match(IWorkViewModel context)
    {
        return context is IllustrationItemViewModel { IsManga: false, IsUgoira: false };
    }
}
