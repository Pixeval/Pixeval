// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

/// <remarks>
/// 包含R18G
/// </remarks>
[MetaPathMacro]
public class IsR18Macro : IPredicate<IArtworkInfo>
{
    public string Name => "is_r18";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsR18);

    public bool Match(IArtworkInfo context) => context.SafeRating.IsR18 || context.SafeRating.IsR18G;
}
