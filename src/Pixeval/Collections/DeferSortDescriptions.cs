// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;

namespace Pixeval.Collections;

public abstract class DeferSortDescriptions<T>
{
    public DeferSortDescriptions()
    {
        SortDescriptions.CollectionChanged += (_, _) => SortChanged();
    }

    private int _sortDescriptionsDeferCount;

    public IDisposable DeferSortDescriptionsChange()
    {
        var defer = new SimpleDefer<T>(this);
        defer.DeferEnded += (_, _) => SortChanged();
        return defer;
    }

    protected void SortChanged()
    {
        if (_sortDescriptionsDeferCount is 0)
            SortChangedOverride();
    }

    /// <summary>
    /// Gets SortDescriptions to sort the visible items
    /// </summary>
    public ObservableCollection<ISortDescription<T>> SortDescriptions { get; } = [];

    protected abstract void SortChangedOverride();

    private class SimpleDefer<TInternal> : IDisposable
    {
        private readonly DeferSortDescriptions<TInternal> _aoc;

        public SimpleDefer(DeferSortDescriptions<TInternal> aoc)
        {
            _aoc = aoc;
            ++aoc._sortDescriptionsDeferCount;
        }

        public event EventHandler? DeferEnded;

        public void Dispose()
        {
            --_aoc._sortDescriptionsDeferCount;
            DeferEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}
