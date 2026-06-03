// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示整数原始值。
/// </summary>
public sealed record FilterRawLongValue(long Value, FilterTextSpan Span) : FilterValue(Span);
