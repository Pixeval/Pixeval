using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkStartDateFilterSyntax : FilterDateSyntax
{
    /// <summary>
    /// 起始日期筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.StartDate;

    public override string? ExampleValue => "2024-1-1";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("s", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.CompletionsStartDate)),
        FilterSyntaxPattern.Keyword("start", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.CompletionsStartDate)),
    ];
}