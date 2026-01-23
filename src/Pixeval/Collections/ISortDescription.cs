// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Collections;

public interface ISortDescription<in T>
{
    [MemberNotNullWhen(true, nameof(PropertyName))]
    [MemberNotNullWhen(false, nameof(Comparer))]
    bool PropertyMode { get; }

    /// <summary>
    /// Gets the name of property to sort on
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    /// Gets the direction of sort
    /// </summary>
    bool IsDescending { get; }

    /// <summary>
    /// Gets the comparer
    /// </summary>
    IComparer<T>? Comparer { get; }
}
