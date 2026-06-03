// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示整数范围原始值。
/// </summary>
public sealed record FilterRawLongRangeValue(FilterParsedRange<long> Value, FilterTextSpan Span) : FilterValue(Span);
