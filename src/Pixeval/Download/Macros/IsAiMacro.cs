// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class IsAiMacro : IPredicate<IArtworkInfo>
{
    public bool IsNot { get; set; }

    public string Name => "if_ai";

    public bool Match(IArtworkInfo context) => context.IsAiGenerated;
}
