// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;

namespace Pixeval.Collections;

/// <summary>
/// Sort description
/// </summary>
public class SortDescription<T> : ISortDescription<T>
{
    /// <inheritdoc />
    public bool PropertyMode { get; }

    /// <inheritdoc />
    public string? PropertyName { get; }

    /// <inheritdoc />
    public bool IsDescending { get; }

    /// <inheritdoc />
    public IComparer<T>? Comparer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortDescription{T}"/> class that describes
    /// a sort on the object itself
    /// </summary>
    /// <param name="comparer">Comparer to use. If null, will use default comparer</param>
    /// <param name="isDescending">Direction of sort</param>
    public SortDescription(IComparer<T>? comparer, bool isDescending)
    {
        IsDescending = isDescending;
        Comparer = comparer;
        PropertyMode = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortDescription{T}"/> class.
    /// </summary>
    /// <param name="propertyName">Name of property to sort on</param>
    /// <param name="isDescending">Direction of sort</param>
    public SortDescription(string propertyName, bool isDescending)
    {
        PropertyName = propertyName;
        IsDescending = isDescending;
        PropertyMode = true;
    }
}
