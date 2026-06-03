using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;
using Pixeval.Filters.Values;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkRatioFilterSyntax : FilterDoubleRangeSyntax
{
    /// <summary>
    /// 宽高比筛选语法，支持整数、小数和分数范围。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Ratio;

    public override string? ExampleValue => "1-2";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("r", exampleValue: "1-2", description: I18NManager.GetResource(FilterResources.CompletionsRatio)),
        FilterSyntaxPattern.Keyword("ratio", exampleValue: "1-2", description: I18NManager.GetResource(FilterResources.CompletionsRatio))
    ];
}