// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class IsSeriesGroupMacro : IPredicate<WorkSubscriptionDownloadContext>
{
    public string Name => "is_series_group";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionIsSeriesGroup);

    public bool Match(WorkSubscriptionDownloadContext context) =>
        context is { IsGroup: true, SubscriptionType: WorkSubscriptionType.Series };
}
