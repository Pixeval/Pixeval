// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示已经绑定到语义的整数范围。
/// </summary>
public readonly record struct FilterLongRange(long Start, long End, bool IsOpenEnded)
{
    /// <summary>
    /// 判断指定整数是否落在当前范围内。
    /// </summary>
    public bool Contains(long value) => value >= Start && (IsOpenEnded || value <= End);

    /// <summary>
    /// 将原始整数范围绑定为通用闭区间语义。
    /// </summary>
    public static bool TryCreate(
        FilterParsedRange<long> raw,
        FilterTextSpan span,
        string syntaxName,
        out FilterLongRange range,
        out FilterDiagnostic? diagnostic)
    {
        var start = raw.Start ?? 0;
        if (raw.Start is not null && !raw.IsStartInclusive)
            ++start;

        if (start < 0)
        {
            range = default;
            diagnostic = new(FilterDiagnosticKind.NegativeRangeUnsupported, span, syntaxName, start);
            return false;
        }

        if (raw.End is null)
        {
            range = new(start, 0, true);
            diagnostic = null;
            return true;
        }

        var end = raw.End.Value;
        if (!raw.IsEndInclusive)
            --end;

        if (end < start)
        {
            range = default;
            diagnostic = new(FilterDiagnosticKind.RangeMinimumGreaterThanMaximum, span, syntaxName, start, end);
            return false;
        }

        range = new(start, end, false);
        diagnostic = null;
        return true;
    }

    /// <summary>
    /// 将原始整数范围绑定为从 1 开始的视图索引范围。
    /// </summary>
    public static bool TryCreateIndexRange(
        FilterParsedRange<long> raw,
        FilterTextSpan span,
        string syntaxName,
        out Range range,
        out FilterDiagnostic? diagnostic)
    {
        range = Range.All;

        if (raw.Start is < 1 || raw.End is < 1)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeStartsFromOne, span, syntaxName, raw.Start, raw.End);
            return false;
        }

        var start = raw.Start is { } lower ? lower - 1 + (raw.IsStartInclusive ? 0 : 1) : 0;
        if (start < 0)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeStartsFromOne, span, syntaxName, raw.Start, raw.End);
            return false;
        }

        if (start > int.MaxValue)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeTooLarge, span, syntaxName, raw.Start);
            return false;
        }

        if (raw.End is null)
        {
            range = new Range((int) start, Index.End);
            diagnostic = null;
            return true;
        }

        var exclusiveEnd = raw.End.Value + (raw.IsEndInclusive ? 0 : -1);
        if (exclusiveEnd <= start)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeMinimumGreaterThanMaximum, span, syntaxName, raw.Start, raw.End);
            return false;
        }

        if (exclusiveEnd > int.MaxValue)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeTooLarge, span, syntaxName, raw.End);
            return false;
        }

        range = new Range((int) start, (int) exclusiveEnd);
        diagnostic = null;
        return true;
    }
}
