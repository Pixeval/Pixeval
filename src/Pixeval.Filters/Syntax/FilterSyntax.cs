// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Text;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示一个可由外部注册的过滤语法定义。
/// </summary>
public abstract class FilterSyntax
{
    public abstract string Key { get; }

    public abstract FilterValueKind ValueKind { get; }

    public abstract IReadOnlyList<FilterSyntaxPattern> Patterns { get; }

    public virtual string? ExampleValue => null;

    internal abstract bool TryCreatePredicate(
        FilterSyntaxMatch match,
        FilterValue rawValue,
        FilterTextSpan termSpan,
        bool isNegated,
        out FilterPredicateNode? predicate,
        out FilterDiagnostic? diagnostic);
}

/// <summary>
/// 表示将原始值绑定为 <typeparamref name="TValue" />，并可作用于 <typeparamref name="TContext" /> 的过滤语法。
/// </summary>
/// <typeparam name="TContext">筛选时使用的上下文类型。</typeparam>
/// <typeparam name="TValue">绑定后的语义值类型。</typeparam>
public abstract class FilterSyntax<TContext, TValue> : FilterSyntax
{
    /// <summary>
    /// 判断当前语法绑定的值是否匹配指定上下文。
    /// </summary>
    public abstract bool Match(TContext context, TValue value);

    /// <summary>
    /// 将原始值绑定为当前语法对应的强类型语义值。
    /// </summary>
    internal sealed override bool TryCreatePredicate(
        FilterSyntaxMatch match,
        FilterValue rawValue,
        FilterTextSpan termSpan,
        bool isNegated,
        out FilterPredicateNode? predicate,
        out FilterDiagnostic? diagnostic)
    {
        if (TryBindCore(match, rawValue, out var value, out diagnostic))
        {
            predicate = new FilterPredicateNode<TContext, TValue>(this, value, termSpan, isNegated);
            diagnostic = null;
            return true;
        }

        predicate = null;
        diagnostic ??= new(
            FilterDiagnosticKind.InvalidValue,
            rawValue.Span.Length > 0 ? rawValue.Span : termSpan,
            match.DiagnosticText);
        return false;
    }

    /// <summary>
    /// 由具体语法实现真实的值绑定逻辑。
    /// </summary>
    protected abstract bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out TValue value, out FilterDiagnostic? diagnostic);
}
