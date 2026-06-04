// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Pixeval.Collections;

public interface IDeferredCollection<T>
{
    ObservableCollection<ISortDescription<T>> SortDescriptions { get; }

    ObservableCollection<IFilter<T>> Filters { get; }

    FilterCombinationMode FilterCombinationMode { get; set; }

    SimpleDefer<T> DeferSortDescriptionsChange();

    SimpleDefer<T> DeferFiltersChange();
}

public interface IAdvancedObservableView<T>
    : IDeferredCollection<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IIncrementalLoading, IDisposable
{
    bool IsReversed { get; set; }

    Range Range { get; set; }

    int IndexOf(T item);
}

public abstract class DeferredCollectionBase<T> : IComparer<T>, IDeferredCollection<T>
{
    protected DeferredCollectionBase()
    {
        SortDescriptions.CollectionChanged += (_, _) => SortChanged();
        Filters.CollectionChanged += (_, _) => FilterChanged();
    }

    private int _sortDescriptionsDeferCount;
    private int _filtersDeferCount;

    public SimpleDefer<T> DeferSortDescriptionsChange()
    {
        var defer = new SimpleDefer<T>(ref _sortDescriptionsDeferCount);
        defer.DeferEnded += (_, _) => SortChanged();
        return defer;
    }

    public SimpleDefer<T> DeferFiltersChange()
    {
        var defer = new SimpleDefer<T>(ref _filtersDeferCount);
        defer.DeferEnded += (_, _) => FilterChanged();
        return defer;
    }

    protected void SortChanged()
    {
        if (_sortDescriptionsDeferCount is 0)
            SortChangedOverride();
    }

    protected void FilterChanged()
    {
        if (_filtersDeferCount is 0)
            FilterChangedOverride();
    }

    /// <summary>
    /// Gets SortDescriptions to sort the visible items
    /// </summary>
    public ObservableCollection<ISortDescription<T>> SortDescriptions { get; } = [];

    public ObservableCollection<IFilter<T>> Filters { get; } = [];

    public FilterCombinationMode FilterCombinationMode
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            FilterChanged();
        }
    } = FilterCombinationMode.And;

    protected bool MatchesFilter(T item)
    {
        if (Filters.Count is 0)
            return true;

        return FilterCombinationMode is FilterCombinationMode.And
            ? Filters.All(MatchesSingleFilter)
            : Filters.Any(MatchesSingleFilter);

        bool MatchesSingleFilter(IFilter<T> filter)
        {
            var result = filter.Predicate(item);
            return filter.IsReversed ? !result : result;
        }
    }

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    int IComparer<T>.Compare(T? x, T? y) => CompareCore(x, y);

    protected int CompareCore(T? x, T? y)
    {
        foreach (var sd in SortDescriptions)
            if (sd.Comparison(x, y) is var cmp and not 0)
                return sd.IsDescending ? -cmp : +cmp;

        return 0;
    }


    protected abstract void SortChangedOverride();

    protected abstract void FilterChangedOverride();
}

public enum FilterCombinationMode
{
    And,
    Or
}

public ref struct SimpleDefer<TInternal> : IDisposable
{
    public SimpleDefer(ref int deferCount)
    {
        ++deferCount;
        _deferCount = ref deferCount;
    }

    private readonly ref int _deferCount;

    public event EventHandler<SimpleDefer<TInternal>, EventArgs>? DeferEnded;

    public void Dispose()
    {
        --_deferCount;
        DeferEnded?.Invoke(this, EventArgs.Empty);
    }
}
