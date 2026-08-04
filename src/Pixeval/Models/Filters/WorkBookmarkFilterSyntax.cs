using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Values;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkBookmarkFilterSyntax : FilterLongRangeSyntax<IArtworkInfo>
{
    public const string KeyConst = "Bookmark";

    public override string Key => KeyConst;

    public override string? ExampleValue => "100-200";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("l", exampleValue: "100-200", description: I18NManager.GetResource(FilterResources.Completions.Bookmark)),
        FilterSyntaxPattern.Keyword("like", exampleValue: "100-200", description: I18NManager.GetResource(FilterResources.Completions.Bookmark))
    ];

    public override bool Match(IArtworkInfo context, FilterLongRange value) => value.Contains(context.TotalFavorite);
}
