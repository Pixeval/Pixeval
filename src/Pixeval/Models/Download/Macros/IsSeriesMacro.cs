// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Model;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsSeriesMacro : IPredicate<IArtworkInfo>
{
    public const string NameConst = "is_series";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsSeries);

    public bool Match(IArtworkInfo context) => context is WorkBase { Series: not null };
}
