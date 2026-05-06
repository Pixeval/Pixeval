// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pixeval.Collections;

public interface IFilter<in T>
{
    ISet<string> ObservedProperties { get; }

    bool IsReversed { get; }

    Predicate<T> Predicate { get; }

    static IFilter<T> Create(Predicate<T> predicate, bool isReversed)
    {
        return new Filter<T>(ReadOnlySet<string>.Empty, predicate, isReversed);
    }

    static IFilter<T> Create(ISet<string> observedProperties, Predicate<T> predicate, bool isReversed)
    {
        return new Filter<T>(observedProperties, predicate, isReversed);
    }

    static IFilter<T> CreateFromProperty(Expression<Func<T, bool>> propertyExpression, bool isReversed)
    {
        if (propertyExpression.Body is not MemberExpression { NodeType: ExpressionType.MemberAccess } memberExpression)
            throw new ArgumentException("The expression must be a member expression.", nameof(propertyExpression));
        var propertyName = memberExpression.Member.Name;
        var propertyGetter = propertyExpression.Compile();
        var predicate = new Predicate<T>(x => propertyGetter(x));
        return new Filter<T>(new[] { propertyName }.ToFrozenSet(), predicate, isReversed);
    }
}

file record Filter<T>(ISet<string> ObservedProperties, Predicate<T> Predicate, bool IsReversed) : IFilter<T>;
