using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Values;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkRatioFilterSyntax : FilterDoubleRangeSyntax<IArtworkInfo>
{
    public const string KeyConst = "Ratio";

    /// <summary>
    /// 宽高比筛选语法，支持整数、小数和分数范围。
    /// </summary>
    public override string Key => KeyConst;

    public override string? ExampleValue => "1-2";

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        FilterSyntaxPattern.Keyword("r", exampleValue: "1-2", description: I18NManager.GetResource(FilterResources.Completions.Ratio)),
        FilterSyntaxPattern.Keyword("ratio", exampleValue: "1-2", description: I18NManager.GetResource(FilterResources.Completions.Ratio))
    ];

    public override bool Match(IArtworkInfo context, FilterDoubleRange value) =>
        context is not IImageSize image || value.Contains(image.AspectRatio);
}
