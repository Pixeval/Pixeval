// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Pixeval.Filters;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Syntax;
using Pixeval.I18N;

namespace Pixeval.Models.Filters;

/// <summary>
/// 提供作品列表使用的筛选语言单例。
/// </summary>
public static class WorkFilterLanguage
{
    private static readonly IReadOnlyList<FilterCompletionDefinition> _IntrinsicCompletions =
    [
        new("builtin.and", "and", "and ", I18NManager.GetResource(FilterResources.CompletionsAnd)),
        new("builtin.or", "or", "or ", I18NManager.GetResource(FilterResources.CompletionsOr)),
        new("builtin.not", "!", "!", I18NManager.GetResource(FilterResources.CompletionsNot))
    ];

    private static readonly IReadOnlyList<FilterFullCompletionDefinition> _FullCompletions =
    [
        new("work.constraint.include", "+ai", "+", I18NManager.GetResource(FilterResources.CompletionsIncludeConstraint), CoveredSyntaxPrefixes: ["+"]),
        new("work.constraint.exclude", "-ai", "-", I18NManager.GetResource(FilterResources.CompletionsExcludeConstraint), CoveredSyntaxPrefixes: ["-"])
    ];

    private static readonly IReadOnlyDictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>> _ValueHintCompletions =
        new Dictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>>
        {
            [FilterValueKind.Text] =
            [
                new("hint.text.plain", "abc", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsTextPlain)),
                new("hint.text.quoted", "\"ab# c\"", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsTextQuoted)),
                new("hint.text.exact", "abc$", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsTextExact)),
                new("hint.text.quoted-exact", "\"ab c$\"", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsTextQuotedExact))
            ],
            [FilterValueKind.Long] =
            [
                new("hint.long.plain", "12345", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsLongPlain))
            ],
            [FilterValueKind.Double] =
            [
                new("hint.double.integer", "2", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleInteger)),
                new("hint.double.decimal", "1.5", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleDecimal)),
                new("hint.double.fraction", "1/2", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleFraction))
            ],
            [FilterValueKind.LongRange] =
            [
                new("hint.long-range.lower", "2-", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsLongRangeLower)),
                new("hint.long-range.upper", "-3", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsLongRangeUpper)),
                new("hint.long-range.closed", "2-3", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsLongRangeClosed))
            ],
            [FilterValueKind.DoubleRange] =
            [
                new("hint.double-range.lower", "2-", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleRangeLower)),
                new("hint.double-range.upper-decimal", "-1.5", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleRangeUpperDecimal)),
                new("hint.double-range.upper-fraction", "-1/2", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleRangeUpperFraction)),
                new("hint.double-range.closed-fraction", "1/2-3", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleRangeClosedFraction)),
                new("hint.double-range.closed-decimal-fraction", "0.3-1/2", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDoubleRangeClosedDecimalFraction))
            ],
            [FilterValueKind.Date] =
            [
                new("hint.date.month-day-dash", "MM-dd", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDateMonthDayDash)),
                new("hint.date.month-day-dot", "MM.dd", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDateMonthDayDot)),
                new("hint.date.full-dash", "yyyy-MM-dd", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDateFullDash)),
                new("hint.date.full-dot", "yyyy.MM.dd", "", I18NManager.GetResource(FilterResources.CompletionsValueHintsDateFullDot))
            ]
        };

    /// <summary>
    /// 汇总所有作品筛选语法后的语言实例。
    /// </summary>
    public static FilterLanguage Instance { get; } = new(
        FilterSyntaxAttributeHelper.GetIWorkViewModelInstances(),
        _IntrinsicCompletions,
        _FullCompletions,
        _ValueHintCompletions);
}
