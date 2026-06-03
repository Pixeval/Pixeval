// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一个逻辑分组节点。
/// </summary>
public sealed record FilterGroupNode(FilterLogicalOperator Operator, IReadOnlyList<FilterNode> Children, FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated);
