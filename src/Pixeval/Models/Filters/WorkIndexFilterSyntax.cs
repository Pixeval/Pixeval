using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkIndexFilterSyntax : FilterLongRangeSyntax
{
    /// <summary>
    /// 视图索引范围语法，用于限制当前列表展示区间。
    /// </summary>
    protected override FilterLongRangeBindingMode BindingMode => FilterLongRangeBindingMode.OneBasedIndex;

    public override string Key => WorkFilterSyntaxKeys.Index;

    public override FilterTermRole Role => FilterTermRole.ViewRange;

    public override string? ExampleValue => "1-3";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("i", exampleValue: "1-3", description: I18NManager.GetResource(FilterResources.CompletionsIndex)),
        FilterSyntaxPattern.Keyword("index", exampleValue: "1-3", description: I18NManager.GetResource(FilterResources.CompletionsIndex))
    ];
}
