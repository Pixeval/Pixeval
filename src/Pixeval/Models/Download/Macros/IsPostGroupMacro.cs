// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Database;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsPostGroupMacro : IPredicate<WorkSubscriptionEntry?>
{
    public const string NameConst = "is_post_group";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsPostGroup);

    public bool Match(WorkSubscriptionEntry? context) =>
        context is { SubscriptionType: WorkSubscriptionType.Posts };
}
