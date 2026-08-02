using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkR18FilterSyntax : FilterFlagSyntax<IArtworkInfo>
{
    public const string KeyConst = "R18";

    /// <summary>
    /// R18 布尔筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["r18"], Metadata: false, Description: I18NManager.GetResource(FilterResources.Completions.Include.R18)),
        new("-", ["r18"], Metadata: true, Description: I18NManager.GetResource(FilterResources.Completions.Exclude.R18))
    ];

    public override bool Match(IArtworkInfo context, bool value) => value ^ (context.SafeRating.IsR18 || context.SafeRating.IsR18G);
}
