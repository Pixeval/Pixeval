// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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

}
