using System;
using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkEndDateFilterSyntax : FilterDateSyntax<IArtworkInfo>
{
    public const string KeyConst = "EndDate";

    /// <summary>
    /// 结束日期筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override string? ExampleValue => "2024-1-1";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("e", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.Completions.EndDate)),
        FilterSyntaxPattern.Keyword("end", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.Completions.EndDate)),
    ];

    public override bool Match(IArtworkInfo context, DateTimeOffset value) => context.CreateDate < value;
}
