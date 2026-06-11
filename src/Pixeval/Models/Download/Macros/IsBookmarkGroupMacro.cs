// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsBookmarkGroupMacro : IPredicate<WorkSubscriptionDownloadContext>
{
    public string Name => "is_bookmark_group";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsBookmarkGroup);

    public bool Match(WorkSubscriptionDownloadContext context) =>
        context is { IsGroup: true, SubscriptionType: WorkSubscriptionType.Bookmarks };
}
