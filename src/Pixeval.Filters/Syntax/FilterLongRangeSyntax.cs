// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取整数范围值的语法。
/// </summary>
public abstract class FilterLongRangeSyntax : FilterSyntax
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.LongRange;

    /// <summary>
    /// 将原始整数范围转换为闭区间语义对象。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawLongRangeValue range)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedLongRangeValue, rawValue.Span, match.DiagnosticText);
            return false;
        }

        if (FilterLongRange.TryCreate(range.Value, rawValue.Span, match.DiagnosticText, out var inclusiveRange, out diagnostic))
        {
            value = inclusiveRange;
            return true;
        }

        value = null;
        return false;
    }
}
