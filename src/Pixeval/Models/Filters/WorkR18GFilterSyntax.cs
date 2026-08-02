using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkR18GFilterSyntax : FilterFlagSyntax<IArtworkInfo>
{
    public const string KeyConst = "R18G";

    /// <summary>
    /// R18G 布尔筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["r18g"], Metadata: false, Description: I18NManager.GetResource(FilterResources.Completions.Include.R18G)),
        new("-", ["r18g"], Metadata: true, Description: I18NManager.GetResource(FilterResources.Completions.Exclude.R18G))
    ];

    public override bool Match(IArtworkInfo context, bool value) => value ^ context.SafeRating.IsR18G;
}
