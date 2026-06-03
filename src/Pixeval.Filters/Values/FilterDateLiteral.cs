// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Filters.Values;

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
