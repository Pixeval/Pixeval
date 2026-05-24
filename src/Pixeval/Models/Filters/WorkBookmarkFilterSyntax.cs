using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkBookmarkFilterSyntax : FilterLongRangeSyntax
{
    /// <summary>
    /// 收藏数过滤语法，使用简单的整数范围表达式。
    /// </summary>
    protected override FilterLongRangeBindingMode BindingMode => FilterLongRangeBindingMode.Inclusive;

    public override string Key => WorkFilterSyntaxKeys.Bookmark;

    public override string? ExampleValue => "100-200";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("l", exampleValue: "100-200", description: I18NManager.GetResource(FilterResources.CompletionsBookmark)),
        FilterSyntaxPattern.Keyword("like", exampleValue: "100-200", description: I18NManager.GetResource(FilterResources.CompletionsBookmark))
    ];
}
