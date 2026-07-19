// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Database;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsGroupMacro : IPredicate<WorkSubscriptionEntry?>
{
    public const string NameConst = "is_group";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsGroup);

    public bool Match(WorkSubscriptionEntry? context) => context is not null;
}
