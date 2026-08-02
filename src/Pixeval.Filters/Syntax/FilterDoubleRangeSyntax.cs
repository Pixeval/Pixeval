// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取小数范围值的语法。
/// </summary>
public abstract class FilterDoubleRangeSyntax<TContext> : FilterSyntax<TContext, FilterDoubleRange>
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.DoubleRange;

    /// <summary>
    /// 将原始小数范围绑定为具体小数范围语义。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out FilterDoubleRange value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawDoubleRangeValue range)
        {
            value = default;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedDoubleRangeValue, rawValue.Span, match.DiagnosticText);
            return false;
        }

        if (FilterDoubleRange.TryCreate(range.Value, rawValue.Span, match.DiagnosticText, out var decimalRange, out diagnostic))
        {
            value = decimalRange;
            return true;
        }

        value = default;
        return false;
    }
}
