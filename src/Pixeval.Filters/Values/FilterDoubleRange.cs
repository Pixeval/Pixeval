// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Globalization;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示已经绑定到语义的小数范围。
/// </summary>
public readonly record struct FilterDoubleRange(double Start, double End, bool IsOpenEnded)
{
    /// <summary>
    /// 判断指定小数是否落在当前范围内。
    /// </summary>
    public bool Contains(double value) => value >= Start && (IsOpenEnded || value <= End);

    /// <summary>
    /// 将原始小数范围绑定为可执行的小数范围。
    /// </summary>
    public static bool TryCreate(
        FilterParsedRange<double> raw,
        FilterTextSpan span,
        string syntaxName,
        out FilterDoubleRange range,
        out FilterDiagnostic? diagnostic)
    {
        if (raw.Start is not null && !raw.IsStartInclusive || raw.End is not null && !raw.IsEndInclusive)
        {
            range = default;
            diagnostic = new(FilterDiagnosticKind.DoubleRangeOpenIntervalUnsupported, span, syntaxName);
            return false;
        }

        var start = raw.Start ?? 0d;
        if (raw.End is null)
        {
            range = new(start, 0d, true);
            diagnostic = null;
            return true;
        }

        var end = raw.End.Value;
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
    /// 将当前小数范围格式化为用户可读文本。
    /// </summary>
    public override string ToString() => IsOpenEnded
        ? string.Create(CultureInfo.InvariantCulture, $"{Start}-")
        : string.Create(CultureInfo.InvariantCulture, $"{Start}-{End}");
}
