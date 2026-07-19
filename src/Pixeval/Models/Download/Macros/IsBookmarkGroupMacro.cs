// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Database;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsBookmarkGroupMacro : IPredicate<WorkSubscriptionEntry?>
{
    public const string NameConst = "is_bookmark_group";

    public string Name => NameConst;

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsBookmarkGroup);

    public bool Match(WorkSubscriptionEntry? context) =>
        context is { SubscriptionType: WorkSubscriptionType.Bookmarks };
}
