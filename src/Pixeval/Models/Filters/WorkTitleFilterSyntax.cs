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
internal sealed class WorkTitleFilterSyntax : FilterTextSyntax
{
    /// <summary>
    /// 标题筛选语法，支持默认文本和 title: 前缀。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Title;

    public override string? ExampleValue => "keyword";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Default("keyword", I18NManager.GetResource(FilterResources.CompletionsTitle)),
        FilterSyntaxPattern.Keyword("title", exampleValue: "keyword", description: I18NManager.GetResource(FilterResources.CompletionsTitle))
    ];
}
