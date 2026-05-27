using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkR18GFilterSyntax : FilterFlagSyntax
{
    /// <summary>
    /// R18G 布尔筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.R18G;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["r18g"], Metadata: false, Description: I18NManager.GetResource(FilterResources.CompletionsIncludeR18G)),
        new("-", ["r18g"], Metadata: true, Description: I18NManager.GetResource(FilterResources.CompletionsExcludeR18G))
    ];
}