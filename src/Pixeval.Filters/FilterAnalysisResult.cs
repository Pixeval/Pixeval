// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Filters;

/// <summary>
/// 描述过滤解析阶段可能出现的诊断类型。
/// </summary>
public enum FilterDiagnosticKind
{
    UnexpectedToken,
    MissingPredicateAfterNegation,
    MissingTextValue,
    MissingRangeValue,
    MissingDateValue,
    MissingGroupOperator,
    MissingRightParenthesis,
    InvalidValue,
    DuplicateViewRange,
    UnsupportedNegation,
    MissingStringQuote,
    InvalidLongRangeFormat,
    InvalidDoubleRangeFormat,
    DateRequiresMonthAndDay,
    ExpectedInteger,
    IntegerOutOfRange,
    DenominatorCannotBeZero,
    MissingFractionalPart,
    InvalidDoubleValue,
    DateValueTooLarge,
    NegativeRangeUnsupported,
    RangeMinimumGreaterThanMaximum,
    IndexRangeStartsFromOne,
    IndexRangeTooLarge,
    IndexRangeMinimumGreaterThanMaximum,
    DoubleRangeOpenIntervalUnsupported,
    InvalidDate,
    UnsupportedValueKind,
    InternalExpectedTextValue,
    InternalExpectedLongRangeValue,
    InternalExpectedDoubleRangeValue,
    InternalExpectedDateValue,
    InternalUnsupportedLongRangeBindingMode,
    InternalViewRangeBindingFailed
}

/// <summary>
/// 表示一条带位置的结构化诊断信息。
/// </summary>
public sealed record FilterDiagnostic(
    FilterDiagnosticKind Kind,
    FilterTextSpan Span,
    params IReadOnlyList<object?> Arguments);

/// <summary>
/// 表示一条可用于自动补全的候选项。
/// </summary>
public sealed record FilterCompletionItem(string DisplayText, string InsertText, FilterTextSpan ReplacementSpan, string? Description = null);

/// <summary>
/// 表示由外部注入到过滤语言中的静态补全定义。
/// </summary>
public sealed record FilterCompletionDefinition(string Key, string DisplayText, string InsertText, string? Description = null);

/// <summary>
/// 描述一次位于语法值内部的补全请求上下文。
/// </summary>
public sealed record FilterValueCompletionContext(
    FilterSyntaxMatch Match,
    string Source,
    FilterTextSpan TokenSpan,
    FilterTextSpan ValueSpan,
    FilterTextSpan FragmentSpan,
    bool IsNegated);

/// <summary>
/// 根据当前语法值上下文提供外部补全项。
/// </summary>
public delegate IReadOnlyList<FilterCompletionDefinition>? FilterValueCompletionProvider(FilterValueCompletionContext context);

/// <summary>
/// 表示一次过滤分析的完整结果。
/// </summary>
public sealed record FilterAnalysisResult(FilterQuery? Query, IReadOnlyList<FilterDiagnostic> Diagnostics, IReadOnlyList<FilterCompletionItem> Completions)
{
    public static FilterAnalysisResult Empty { get; } = new(FilterQuery.Empty, [], []);

    public bool IsSuccess => Query is not null && Diagnostics.Count is 0;
}
