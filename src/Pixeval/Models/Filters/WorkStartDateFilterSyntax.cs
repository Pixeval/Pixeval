using System;
using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkStartDateFilterSyntax : FilterDateSyntax<IArtworkInfo>
{
    public const string KeyConst = "StartDate";

    /// <summary>
    /// 起始日期筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override string? ExampleValue => "2024-1-1";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("s", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.Completions.StartDate)),
        FilterSyntaxPattern.Keyword("start", exampleValue: "2024-1-1", description: I18NManager.GetResource(FilterResources.Completions.StartDate)),
    ];

    public override bool Match(IArtworkInfo context, DateTimeOffset value) => context.CreateDate >= value;
}
