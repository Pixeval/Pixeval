using System.Collections.Generic;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkAuthorFilterSyntax : FilterTextSyntax
{
    /// <summary>
    /// 作者筛选语法，支持 @、a: 和 artist: 写法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Author;

    public override string? ExampleValue => "artist";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.PrefixOnly("@", "artist", I18NManager.GetResource(FilterResources.CompletionsAuthor)),
        FilterSyntaxPattern.Keyword("a", exampleValue: "artist", description: I18NManager.GetResource(FilterResources.CompletionsAuthor)),
        FilterSyntaxPattern.Keyword("artist", exampleValue: "artist", description: I18NManager.GetResource(FilterResources.CompletionsAuthor))
    ];
}