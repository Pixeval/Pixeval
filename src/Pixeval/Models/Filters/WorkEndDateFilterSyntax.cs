using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkEndDateFilterSyntax : FilterDateSyntax
{
    /// <summary>
    /// 结束日期筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.EndDate;

    public override string? ExampleValue => "2024-1-1";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("e", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.CompletionsEndDate)),
        FilterSyntaxPattern.Keyword("end", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.CompletionsEndDate)),
    ];
}