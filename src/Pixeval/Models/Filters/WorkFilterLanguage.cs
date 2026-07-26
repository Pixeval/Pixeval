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
        new("builtin.and", "and", "and ", I18NManager.GetResource(FilterResources.Completions.And)),
        new("builtin.or", "or", "or ", I18NManager.GetResource(FilterResources.Completions.Or)),
        new("builtin.not", "!", "!", I18NManager.GetResource(FilterResources.Completions.Not))
    ];

    private static readonly IReadOnlyList<FilterFullCompletionDefinition> _FullCompletions =
    [
        new("work.constraint.include", "+ai", "+", I18NManager.GetResource(FilterResources.Completions.Include.Constraint), CoveredSyntaxPrefixes: ["+"]),
        new("work.constraint.exclude", "-ai", "-", I18NManager.GetResource(FilterResources.Completions.Exclude.Constraint), CoveredSyntaxPrefixes: ["-"])
    ];

    private static readonly IReadOnlyDictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>> _ValueHintCompletions =
        new Dictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>>
        {
            [FilterValueKind.Text] =
            [
                new("hint.text.plain", "abc", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Text.Plain)),
                new("hint.text.quoted", "\"ab# c\"", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Text.Quoted)),
                new("hint.text.exact", "abc$", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Text.Exact)),
                new("hint.text.quoted-exact", "\"ab c$\"", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Text.QuotedExact))
            ],
            [FilterValueKind.Long] =
            [
                new("hint.long.plain", "12345", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Long.Plain))
            ],
            [FilterValueKind.Double] =
            [
                new("hint.double.integer", "2", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Double.Integer)),
                new("hint.double.decimal", "1.5", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Double.Decimal)),
                new("hint.double.fraction", "1/2", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Double.Fraction))
            ],
            [FilterValueKind.LongRange] =
            [
                new("hint.long-range.lower", "2-", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.LongRange.Lower)),
                new("hint.long-range.upper", "-3", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.LongRange.Upper)),
                new("hint.long-range.closed", "2-3", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.LongRange.Closed))
            ],
            [FilterValueKind.DoubleRange] =
            [
                new("hint.double-range.lower", "2-", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.DoubleRange.Lower)),
                new("hint.double-range.upper-decimal", "-1.5", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.DoubleRange.UpperDecimal)),
                new("hint.double-range.upper-fraction", "-1/2", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.DoubleRange.UpperFraction)),
                new("hint.double-range.closed-fraction", "1/2-3", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.DoubleRange.ClosedFraction)),
                new("hint.double-range.closed-decimal-fraction", "0.3-1/2", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.DoubleRange.ClosedDecimalFraction))
            ],
            [FilterValueKind.Date] =
            [
                new("hint.date.month-day-dash", "MM-dd", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Date.MonthDayDash)),
                new("hint.date.month-day-dot", "MM.dd", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Date.MonthDayDot)),
                new("hint.date.full-dash", "yyyy-MM-dd", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Date.FullDash)),
                new("hint.date.full-dot", "yyyy.MM.dd", "", I18NManager.GetResource(FilterResources.Completions.ValueHints.Date.FullDot))
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
