using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkGifFilterSyntax : FilterFlagSyntax<IArtworkInfo>
{
    public const string KeyConst = "Gif";

    /// <summary>
    /// 动图布尔筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["gif"], Metadata: false, Description: I18NManager.GetResource(FilterResources.Completions.Include.Gif)),
        new("-", ["gif"], Metadata: true, Description: I18NManager.GetResource(FilterResources.Completions.Exclude.Gif))
    ];

    public override bool Match(IArtworkInfo context, bool value) =>
        value ^ (context.ImageType is ImageType.SingleAnimatedImage);
}
