// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class IsR18GMacro : IPredicate<IArtworkInfo>
{
    public bool IsNot { get; set; }

    public string Name => "if_r18g";

    public bool Match(IArtworkInfo context) => context.SafeRating.IsR18G;
}
