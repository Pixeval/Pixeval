using System.Collections.Generic;
using Misaki;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

[FilterSyntax<IArtworkInfo>]
internal sealed class WorkAiFilterSyntax : FilterFlagSyntax<IArtworkInfo>
{
    public const string KeyConst = "Ai";

    /// <summary>
    /// AI 作品布尔筛选语法。
    /// </summary>
    public override string Key => KeyConst;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["ai"], Metadata: false, Description: I18NManager.GetResource(FilterResources.Completions.Include.Ai)),
        new("-", ["ai"], Metadata: true, Description: I18NManager.GetResource(FilterResources.Completions.Exclude.Ai))
    ];

    public override bool Match(IArtworkInfo context, bool value) => value ^ context.IsAiGenerated;
}
