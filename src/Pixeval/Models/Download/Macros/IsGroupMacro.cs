// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsGroupMacro : IPredicate<WorkSubscriptionDownloadContext>
{
    public string Name => "is_group";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsGroup);

    public bool Match(WorkSubscriptionDownloadContext context) => context.IsGroup;
}
