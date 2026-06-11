// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsAiMacro : IPredicate<IArtworkInfo>
{
    public string Name => "is_ai";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsAi);

    public bool Match(IArtworkInfo context) => context.IsAiGenerated;
}
