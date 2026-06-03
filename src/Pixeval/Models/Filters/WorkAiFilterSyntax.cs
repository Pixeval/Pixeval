using System.Collections.Generic;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkAiFilterSyntax : FilterFlagSyntax
{
    /// <summary>
    /// AI 作品布尔筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Ai;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["ai"], Metadata: false, Description: I18NManager.GetResource(FilterResources.CompletionsIncludeAi)),
        new("-", ["ai"], Metadata: true, Description: I18NManager.GetResource(FilterResources.CompletionsExcludeAi))
    ];
}
