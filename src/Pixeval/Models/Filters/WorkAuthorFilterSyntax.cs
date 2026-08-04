using System.Collections.Generic;
using System.Linq;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Values;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkAuthorFilterSyntax : FilterTextSyntax<IArtworkInfo>
{
    public const string KeyConst = "Author";

    /// <summary>
    /// 作者筛选语法，支持 @、a: 和 artist: 写法。
    /// </summary>
    public override string Key => KeyConst;

    public override string? ExampleValue => "artist";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.PrefixOnly("@", "artist", I18NManager.GetResource(FilterResources.Completions.Author)),
        FilterSyntaxPattern.Keyword("a", exampleValue: "artist", description: I18NManager.GetResource(FilterResources.Completions.Author)),
        FilterSyntaxPattern.Keyword("artist", exampleValue: "artist", description: I18NManager.GetResource(FilterResources.Completions.Author))
    ];

    public override bool Match(IArtworkInfo context, FilterTextValue value) =>
        context.Authors.Any(author => value.Matches(author.Name));
}
