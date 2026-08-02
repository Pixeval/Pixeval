// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一个绑定到具体语法的谓词节点。
/// </summary>
public abstract record FilterPredicateNode(FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated)
{
    public abstract FilterSyntax Syntax { get; }
}

/// <summary>
/// 表示一个带有强类型上下文和值的谓词节点。
/// </summary>
public sealed record FilterPredicateNode<TContext, TValue>(
    FilterSyntax<TContext, TValue> Syntax,
    TValue Value,
    FilterTextSpan Span,
    bool IsNegated = false)
    : FilterPredicateNode(Span, IsNegated)
{
    public override FilterSyntax<TContext, TValue> Syntax { get; } = Syntax;

    /// <inheritdoc />
    public override bool Match(object? context)
    {
        if (context is not TContext typedContext)
            throw new ArgumentException($"Filter syntax '{Syntax.Key}' requires a context assignable to {typeof(TContext)}.", nameof(context));

        return IsNegated ^ Syntax.Match(typedContext, Value);
    }
}
