// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Globalization;

namespace Pixeval.Filters;

/// <summary>
/// 表示语法解析阶段产出的原始值对象。
/// </summary>
public abstract record FilterValue(FilterTextSpan Span);

/// <summary>
/// 表示当前条件不携带额外值。
/// </summary>
public sealed record FilterNoneValue(FilterTextSpan Span) : FilterValue(Span);

/// <summary>
/// 表示字符串过滤值，保留原始文本和精确匹配标记。
/// </summary>
public sealed record FilterTextValue(string Source, FilterTextSpan ContentSpan, bool IsExact, FilterTextSpan Span) : FilterValue(Span)
{
    /// <summary>
    /// 返回当前字符串值对应的只读字符片段。
    /// </summary>
    public ReadOnlySpan<char> AsSpan() => ContentSpan.Slice(Source);

    /// <summary>
    /// 返回当前字符串值的文本内容。
    /// </summary>
    public override string ToString() => ContentSpan.GetText(Source);
}

/// <summary>
/// 表示尚未绑定语义的通用范围值。
/// </summary>
public readonly record struct FilterParsedRange<T>(T? Start, bool IsStartInclusive, T? End, bool IsEndInclusive)
    where T : struct;

/// <summary>
/// 表示整数范围原始值。
/// </summary>
public sealed record FilterRawLongRangeValue(FilterParsedRange<long> Value, FilterTextSpan Span) : FilterValue(Span);

/// <summary>
/// 表示小数范围原始值。
/// </summary>
public sealed record FilterRawDoubleRangeValue(FilterParsedRange<double> Value, FilterTextSpan Span) : FilterValue(Span);

/// <summary>
/// 表示日期原始值。
/// </summary>
public sealed record FilterRawDateValue(FilterDateLiteral Value, FilterTextSpan Span) : FilterValue(Span);

/// <summary>
/// 表示尚未补全年份的日期字面量。
/// </summary>
public readonly record struct FilterDateLiteral(int? Year, int Month, int Day)
{
    /// <summary>
    /// 将日期字面量转换为具体时间点。
    /// </summary>
    public DateTimeOffset ToDateTimeOffset(int fallbackYear)
        => new(new DateTime(Year ?? fallbackYear, Month, Day));

    /// <summary>
    /// 将日期字面量格式化为可读字符串。
    /// </summary>
    public override string ToString() => Year is { } year ? $"{year}-{Month}-{Day}" : $"{Month}-{Day}";
}

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
    public static bool TryCreate(FilterParsedRange<long> raw, FilterTextSpan span, out FilterLongRange range, out FilterDiagnostic? diagnostic)
    {
        var start = raw.Start ?? 0;
        if (raw.Start is not null && !raw.IsStartInclusive)
            ++start;

        if (start < 0)
        {
            range = default;
            diagnostic = new(FilterDiagnosticKind.NegativeRangeUnsupported, span);
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
            diagnostic = new(FilterDiagnosticKind.RangeMinimumGreaterThanMaximum, span);
            return false;
        }

        range = new(start, end, false);
        diagnostic = null;
        return true;
    }

    /// <summary>
    /// 将原始整数范围绑定为从 1 开始的视图索引范围。
    /// </summary>
    public static bool TryCreateIndexRange(FilterParsedRange<long> raw, FilterTextSpan span, out Range range, out FilterDiagnostic? diagnostic)
    {
        range = Range.All;

        if (raw.Start is < 1 || raw.End is < 1)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeStartsFromOne, span);
            return false;
        }

        var start = raw.Start is { } lower ? lower - 1 + (raw.IsStartInclusive ? 0 : 1) : 0;
        if (start < 0)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeStartsFromOne, span);
            return false;
        }

        if (start > int.MaxValue)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeTooLarge, span);
            return false;
        }

        if (raw.End is null)
        {
            range = new Range((int)start, Index.End);
            diagnostic = null;
            return true;
        }

        var exclusiveEnd = raw.End.Value + (raw.IsEndInclusive ? 0 : -1);
        if (exclusiveEnd <= start)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeMinimumGreaterThanMaximum, span);
            return false;
        }

        if (exclusiveEnd > int.MaxValue)
        {
            diagnostic = new(FilterDiagnosticKind.IndexRangeTooLarge, span);
            return false;
        }

        range = new Range((int)start, (int)exclusiveEnd);
        diagnostic = null;
        return true;
    }
}

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
    public static bool TryCreate(FilterParsedRange<double> raw, FilterTextSpan span, out FilterDoubleRange range, out FilterDiagnostic? diagnostic)
    {
        if (raw.Start is not null && !raw.IsStartInclusive || raw.End is not null && !raw.IsEndInclusive)
        {
            range = default;
            diagnostic = new(FilterDiagnosticKind.DoubleRangeOpenIntervalUnsupported, span);
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
            diagnostic = new(FilterDiagnosticKind.RangeMinimumGreaterThanMaximum, span);
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
