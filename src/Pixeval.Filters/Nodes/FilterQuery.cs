// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一次完整过滤查询。
/// </summary>
public sealed record FilterQuery(FilterGroupNode Root, Range ViewRange)
{
    public static FilterQuery Empty { get; } = new(new(FilterLogicalOperator.And, [], FilterTextSpan.EmptyAt(0)), Range.All);

    public bool HasPredicates => Root.Children.Count > 0;
}
