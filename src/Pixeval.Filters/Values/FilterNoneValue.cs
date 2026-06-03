// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示当前条件不携带额外值。
/// </summary>
public sealed record FilterNoneValue(FilterTextSpan Span) : FilterValue(Span);
