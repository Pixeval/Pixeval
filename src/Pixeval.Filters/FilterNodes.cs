// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

/// <summary>
/// 表示过滤语法树中的一个节点。
/// </summary>
public abstract record FilterNode(FilterTextSpan Span, bool IsNegated);

/// <summary>
/// 表示分组节点使用的逻辑运算符。
/// </summary>
public enum FilterLogicalOperator
{
    And,
    Or
}

/// <summary>
/// 表示由多个子条件组成的逻辑分组。
/// </summary>
public sealed record FilterGroupNode(FilterLogicalOperator Operator, IReadOnlyList<FilterNode> Children, FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated);

/// <summary>
/// 表示已经完成绑定的单个过滤条件。
/// </summary>
public sealed record FilterPredicateNode(FilterSyntax Syntax, object? Value, FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated);

/// <summary>
/// 表示一次过滤解析后可直接执行的查询对象。
/// </summary>
public sealed record FilterQuery(FilterGroupNode Root, Range ViewRange)
{
    public static FilterQuery Empty { get; } = new(new(FilterLogicalOperator.And, [], FilterTextSpan.EmptyAt(0)), Range.All);

    public bool HasPredicates => Root.Children.Count > 0;
}
