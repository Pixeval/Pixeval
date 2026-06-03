// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示日期原始值。
/// </summary>
public sealed record FilterRawDateValue(FilterDateLiteral Value, FilterTextSpan Span) : FilterValue(Span);
