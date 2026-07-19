// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Database;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class GroupIdMacro : ITransducer<WorkSubscriptionEntry?>, IContextRestrictedMacro
{
    public const string NameConst = "group_id";

    public string Name => NameConst;

    public MacroContextPredicate ContextPredicate => static context =>
        (context.TryGetValue(IsGroupMacro.NameConst, out var isGroup) && isGroup)
        || (context.TryGetValue(IsBookmarkGroupMacro.NameConst, out var isBookmarkGroup) && isBookmarkGroup)
        || (context.TryGetValue(IsPostGroupMacro.NameConst, out var isPostGroup) && isPostGroup)
        || (context.TryGetValue(IsSeriesGroupMacro.NameConst, out var isSeriesGroup) && isSeriesGroup);

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionGroupId);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsIntegerFormatterValid(formatter);

    public string Substitute(WorkSubscriptionEntry? context, string? formatter, out bool includeToken)
    {
        includeToken = false;
        return context is { Id: var id } ? MacroHelper.FormatInteger(id, formatter) : "";
    }
}
