// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一次完整过滤查询。
/// </summary>
public sealed record FilterQuery(FilterGroupNode Root)
{
    public static FilterQuery Empty { get; } = new(new FilterGroupNode(FilterLogicalOperator.And, [], FilterTextSpan.EmptyAt(0)));

    public bool HasPredicates => Root.Children.Count > 0;
}
