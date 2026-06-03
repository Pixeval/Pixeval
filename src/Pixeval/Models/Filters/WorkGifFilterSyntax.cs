using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;
using Pixeval.Filters.Values;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Models.Filters;

[FilterSyntax<IWorkViewModel>]
internal sealed class WorkGifFilterSyntax : FilterFlagSyntax
{
    /// <summary>
    /// 动图布尔筛选语法。
    /// </summary>
    public override string Key => WorkFilterSyntaxKeys.Gif;

    public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
    [
        new("+", ["gif"], Metadata: false, Description: I18NManager.GetResource(FilterResources.CompletionsIncludeGif)),
        new("-", ["gif"], Metadata: true, Description: I18NManager.GetResource(FilterResources.CompletionsExcludeGif))
    ];
}
