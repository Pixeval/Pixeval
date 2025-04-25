// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

/// <summary>
/// 包含R18G
/// </summary>
[MetaPathMacro<IArtworkInfo>]
public class IsR18Macro : IPredicate<IArtworkInfo>
{
    public bool IsNot { get; set; }

    public string Name => "if_r18";

    public bool Match(IArtworkInfo context) => context.SafeRating.IsR18 || context.SafeRating.IsR18G;
}
