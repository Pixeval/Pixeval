using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkTagFilterSyntax : FilterTextSyntax
{
    /// <summary>
    /// 标签过滤语法，支持 #、t: 和 tag: 写法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Tag;

    public override string? ExampleValue => "tag";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.PrefixOnly("#", "tag", I18NManager.GetResource(FilterResources.CompletionsTag)),
        FilterSyntaxPattern.Keyword("t", exampleValue: "tag", description: I18NManager.GetResource(FilterResources.CompletionsTag)),
        FilterSyntaxPattern.Keyword("tag", exampleValue: "tag", description: I18NManager.GetResource(FilterResources.CompletionsTag))
    ];
}