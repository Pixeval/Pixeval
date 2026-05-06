// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pixeval.Collections;

public interface ISortDescription<in T>
{
    /// <summary>
    /// Gets the name of properties to sort on, if Count is 0, will always sort
    /// </summary>
    ISet<string> ObservedProperties { get; }

    /// <summary>
    /// Gets the comparer
    /// </summary>
    Comparison<T?> Comparison { get; }

    /// <summary>
    /// Gets the direction of sort
    /// </summary>
    bool IsDescending { get; }

    static ISortDescription<T> Create(Comparison<T?> comparison, bool isDescending = false)
    {
        return new SortDescription<T>(ReadOnlySet<string>.Empty, comparison, isDescending);
    }

    static ISortDescription<T> Create(ISet<string> observedProperties, Comparison<T?> comparison, bool isDescending = false)
    {
        return new SortDescription<T>(observedProperties, comparison, isDescending);
    }

    static ISortDescription<T> Create<TProperty>(Expression<Func<T, TProperty>> propertyExpression, bool isDescending = false)
    {
        if (propertyExpression.Body is not MemberExpression { NodeType: ExpressionType.MemberAccess } memberExpression)
            throw new ArgumentException("The expression must be a member expression.", nameof(propertyExpression));
        var propertyName = memberExpression.Member.Name;
        var propertyGetter = propertyExpression.Compile();
        var comparison = new Comparison<T?>((x, y) =>
        {
            if (x is null)
                if (y is null)
                    return 0;
                else
                    return -1;
            if (y is null)
                return 1;

            return Comparer<TProperty>.Default.Compare(propertyGetter(x), propertyGetter(y));
        });
        return new SortDescription<T>(new[] { propertyName }.ToFrozenSet(), comparison, isDescending);
    }

    static ISortDescription<T> Create<TProperty1, TProperty2>(
        Expression<Func<T, TProperty1>> firstBy,
        Expression<Func<T, TProperty2>> thenBy,
        bool isDescending = false)
    {
        if (firstBy.Body is not MemberExpression { NodeType: ExpressionType.MemberAccess } firstMemberExpression)
            throw new ArgumentException("The expression must be a member expression.", nameof(firstBy));
        if (thenBy.Body is not MemberExpression { NodeType: ExpressionType.MemberAccess } thenMemberExpression)
            throw new ArgumentException("The expression must be a member expression.", nameof(thenBy));
        var firstMemberName = firstMemberExpression.Member.Name;
        var thenMemberName = thenMemberExpression.Member.Name;
        var firstPropertyGetter = firstBy.Compile();
        var thenPropertyGetter = thenBy.Compile();
        var comparison = new Comparison<T?>((x, y) =>
        {
            if (x is null)
                if (y is null)
                    return 0;
                else
                    return -1;
            if (y is null)
                return 1;

            if (Comparer<TProperty1>.Default.Compare(firstPropertyGetter(x), firstPropertyGetter(y)) is var firstComparison and not 0)
                return firstComparison;
            return Comparer<TProperty2>.Default.Compare(thenPropertyGetter(x), thenPropertyGetter(y));
        });

        return new SortDescription<T>(new[] { firstMemberName, thenMemberName }.ToFrozenSet(), comparison, isDescending);
    }
}

file record SortDescription<T>(ISet<string> ObservedProperties, Comparison<T?> Comparison, bool IsDescending) : ISortDescription<T>;
