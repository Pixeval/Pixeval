// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsR18GMacro : IPredicate<IArtworkInfo>
{
    public string Name => "is_r18g";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsR18G);

    public bool Match(IArtworkInfo context) => context.SafeRating.IsR18G;
}
