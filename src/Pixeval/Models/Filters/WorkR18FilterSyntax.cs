using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkR18FilterSyntax : FilterFlagSyntax
{
    /// <summary>
    /// R18 布尔筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.R18;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["r18"], Metadata: false, Description: I18NManager.GetResource(FilterResources.CompletionsIncludeR18)),
        new("-", ["r18"], Metadata: true, Description: I18NManager.GetResource(FilterResources.CompletionsExcludeR18))
    ];
}