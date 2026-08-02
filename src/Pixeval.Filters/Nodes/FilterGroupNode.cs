// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一个逻辑分组节点。
/// </summary>
public sealed record FilterGroupNode(FilterLogicalOperator Operator, IReadOnlyList<FilterNode> Children, FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated)
{
    /// <inheritdoc />
    public override bool Match(object? context)
    {
        if (Children.Count is 0)
            return true;

        var matches = Operator switch
        {
            FilterLogicalOperator.And => Children.All(child => child.Match(context)),
            FilterLogicalOperator.Or => Children.Any(child => child.Match(context)),
            _ => throw new ArgumentOutOfRangeException(nameof(Operator))
        };
        return IsNegated ^ matches;
    }
}
