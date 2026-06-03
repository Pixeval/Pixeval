// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取日期值的语法。
/// </summary>
public abstract class FilterDateSyntax : FilterSyntax
{
    protected virtual int FallbackYear => DateTime.UtcNow.Year;

    public sealed override FilterValueKind ValueKind => FilterValueKind.Date;

    /// <summary>
    /// 将原始日期值绑定为具体时间点。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawDateValue date)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedDateValue, rawValue.Span, match.DiagnosticText);
            return false;
        }

        try
        {
            value = date.Value.ToDateTimeOffset(FallbackYear);
            diagnostic = null;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InvalidDate, rawValue.Span, match.DiagnosticText, date.Value);
            return false;
        }
    }
}
