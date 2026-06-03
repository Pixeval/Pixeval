// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示小数范围原始值。
/// </summary>
public sealed record FilterRawDoubleRangeValue(FilterParsedRange<double> Value, FilterTextSpan Span) : FilterValue(Span);
