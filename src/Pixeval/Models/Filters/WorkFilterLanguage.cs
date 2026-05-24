// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Filters;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

/// <summary>
/// 提供作品列表使用的过滤语言单例。
/// </summary>
internal static class WorkFilterLanguage
{
    private static readonly FilterCompletionDefinition[] _IntrinsicCompletions =
    [
        new("builtin.and", "and", "and", I18NManager.GetResource(FilterResources.CompletionsAnd)),
        new("builtin.or", "or", "or", I18NManager.GetResource(FilterResources.CompletionsOr)),
        new("builtin.not", "!", "!", I18NManager.GetResource(FilterResources.CompletionsNot))
    ];

    /// <summary>
    /// 汇总所有作品过滤语法后的语言实例。
    /// </summary>
    public static FilterLanguage Instance { get; } = new(FilterSyntaxAttributeHelper.GetIWorkViewModelInstances(), _IntrinsicCompletions);
}
