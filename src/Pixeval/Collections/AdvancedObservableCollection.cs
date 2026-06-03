// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Collections;

[DebuggerDisplay("Count = {Count}")]
public class AdvancedObservableCollection<T>
    : DeferredCollectionBase<T>, IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IIncrementalLoading, IComparer<T>, IDisposable where T : class
{
    private readonly bool _liveShapingEnabled;

    private readonly List<T> _view = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedObservableCollection{T}"/> class.
    /// </summary>
    public AdvancedObservableCollection() : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedObservableCollection{T}"/> class.
    /// </summary>
    /// <param name="source">source IEnumerable</param>
    /// <param name="isLiveShaping">Denotes whether this AOC should re-filter/re-sort if a PropertyChanged is raised for an observed property.</param>
    public AdvancedObservableCollection(ObservableCollection<T> source, bool isLiveShaping = false)
    {
        _liveShapingEnabled = isLiveShaping;
        Source = source;
    }

    /// <summary>
    /// Gets or sets the source
    /// </summary>
    public ObservableCollection<T> Source
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value, "Null is not allowed");

            if (field == value)
                return;

            DetachSourceHandler(field);
            field = value;
            AttachSourceHandler(field);

            HandleSourceChanged();
            OnPropertyChanged();
        }
    }

    public bool IsReversed
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            SortChanged();
            OnPropertyChanged();
        }
    }

    #region IList<T>

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _view.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _view.GetEnumerator();

    /// <inheritdoc />
    public void Add(T item) => Source.Add(item);

    /// <inheritdoc cref="ICollection{T}.Clear"/> />
    public void Clear() => Source.Clear();

    /// <inheritdoc />
    public bool Contains(T item) => _view.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => _view.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(T item) => Source.Remove(item);

    /// <inheritdoc cref="ICollection{T}.Count"/> />
    public int Count => _view.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int IndexOf(T item) => _view.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        var sourceIndex = index < _view.Count ? Source.IndexOf(_view[index]) : Source.Count;
        Source.Insert(sourceIndex, item);
    }

    /// <inheritdoc cref="IList{T}.RemoveAt"/> />
    public void RemoveAt(int index) => Source.Remove(_view[index]);

    /// <inheritdoc cref="List{T}.this[int]"/>
    public T this[int index]
    {
        get => _view[index];
        set
        {
            var sourceIndex = Source.IndexOf(_view[index]);
            if (sourceIndex >= 0)
                Source[sourceIndex] = value;
        }
    }

    #endregion

    /// <inheritdoc cref="IIncrementalLoading.LoadMoreItemsAsync"/>
    public Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default) => (Source as IIncrementalLoading)?.LoadMoreItemsAsync(count, token) ?? Task.FromResult(0);

    /// <inheritdoc cref="IIncrementalLoading.HasMoreItems"/>
    public bool HasMoreItems => (Source as IIncrementalLoading)?.HasMoreItems ?? false;

    public Range Range
    {
        get;
        set
        {
            if (field.Equals(value))
                return;

            field = value;
            HandleSourceChanged();
        }
    } = Range.All;

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    int IComparer<T>.Compare(T? x, T? y) => CompareCore(x, y);

    private int CompareView(T? x, T? y)
    {
        var comparison = CompareCore(x, y);
        return IsReversed ? -comparison : comparison;
    }

    private IComparer<T> ViewComparer => Comparer<T>.Create(CompareView);

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when the filters change.
    /// </summary>
    public new event EventHandler<AdvancedObservableCollection<T>, EventArgs>? FilterChanged;

    /// <summary>
    /// Property changed event invoker
    /// </summary>
    /// <param name="propertyName">name of the property that changed</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Property changed event invoker
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_liveShapingEnabled)
            return;

        if (string.IsNullOrEmpty(e.PropertyName))
        {
            HandleSourceChanged();
            return;
        }

        for (var sourceIndex = 0; sourceIndex < Source.Count; ++sourceIndex)
        {
            var item = Source[sourceIndex];
            if (!ReferenceEquals(item, sender))
                continue;

            var shouldReevaluateFilter = Filters.Count is not 0
                && Filters.Any(filter => filter.ObservedProperties.Contains(e.PropertyName!));

            if (shouldReevaluateFilter)
            {
                var filterResult = MatchesFilter(item);
                var viewIndex = _view.IndexOf(item);
                if (viewIndex is not -1 && !filterResult)
                    RemoveFromView(viewIndex, item);
                else if (viewIndex is -1 && filterResult && IsSourceIndexInRange(sourceIndex))
                    _ = HandleItemAdded(sourceIndex, item);
            }

            if (MatchesFilter(item)
                && SortDescriptions.Count is not 0
                && SortDescriptions.Any(sd => sd.ObservedProperties.Contains(e.PropertyName!)))
            {
                var oldIndex = _view.IndexOf(item);

                if (oldIndex < 0)
                    continue;

                _view.RemoveAt(oldIndex);
                var targetIndex = GetSortedInsertIndex(item);

                _view.Insert(targetIndex, item);

                if (targetIndex != oldIndex)
                    OnCollectionChanged(new(NotifyCollectionChangedAction.Move, item, targetIndex, oldIndex));
            }
        }
    }

    private void AttachSourceHandler(IEnumerable? items)
    {
        (items as INotifyCollectionChanged)?.CollectionChanged += SourceNcc_CollectionChanged;
        AttachPropertyChangedHandler(items);
    }

    private void AttachPropertyChangedHandler(IEnumerable? items)
    {
        if (!_liveShapingEnabled || items is null)
            return;

        foreach (var item in items.OfType<INotifyPropertyChanged>())
            item.PropertyChanged += ItemOnPropertyChanged;
    }

    private void DetachSourceHandler(IEnumerable? items)
    {
        (items as INotifyCollectionChanged)?.CollectionChanged -= SourceNcc_CollectionChanged;
        DetachPropertyChangedHandler(items);
    }

    private void DetachPropertyChangedHandler(IEnumerable? items)
    {
        if (!_liveShapingEnabled || items is null)
            return;

        foreach (var item in items.OfType<INotifyPropertyChanged>())
            item.PropertyChanged -= ItemOnPropertyChanged;
    }

    protected override void SortChangedOverride()
    {
        ApplyOrderedView(CreateOrderedVisibleView());
    }

    protected override void FilterChangedOverride()
    {
        if (Filters.Count is not 0)
        {
            for (var index = 0; index < _view.Count; ++index)
            {
                var item = _view[index];
                if (MatchesFilter(item))
                    continue;

                RemoveFromView(index, item);
                index--;
            }
        }

        var remaining = new List<T>(_view);
        foreach (var (index, item) in EnumerateRangedSource())
        {
            if (!MatchesFilter(item))
                continue;

            if (remaining.IndexOf(item) is var existingIndex and >= 0)
            {
                remaining.RemoveAt(existingIndex);
                continue;
            }

            _ = HandleItemAdded(index, item);
        }

        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleSourceChanged()
    {
        ResetView(CreateOrderedVisibleView());
    }

    private List<T> CreateOrderedVisibleView()
    {
        var orderedView = new List<T>();
        foreach (var (_, item) in EnumerateRangedSource())
        {
            if (!MatchesFilter(item))
                continue;

            if (SortDescriptions.Count is not 0)
            {
                var targetIndex = orderedView.BinarySearch(item, this);
                if (targetIndex < 0)
                    targetIndex = ~targetIndex;

                orderedView.Insert(targetIndex, item);
            }
            else
                orderedView.Add(item);
        }

        if (IsReversed)
            orderedView.Reverse();

        return orderedView;
    }

    private (int Start, int End) GetSourceRangeBounds()
    {
        if (Equals(Range, Range.All))
            return (0, Source.Count);

        var sourceCount = Source.Count;
        var start = Range.Start.GetOffset(sourceCount);
        if (start > sourceCount)
            return (0, 0);

        var end = Range.End.GetOffset(sourceCount);
        if (end < 0 || start > end)
            return (0, 0);

        return (Math.Max(0, start), Math.Min(sourceCount, end));
    }

    private bool IsSourceIndexInRange(int sourceIndex)
    {
        var (start, end) = GetSourceRangeBounds();
        return sourceIndex >= start && sourceIndex < end;
    }

    private IEnumerable<(int Index, T Item)> EnumerateRangedSource()
    {
        var (start, end) = GetSourceRangeBounds();
        for (var index = start; index < end; ++index)
            yield return (index, Source[index]);
    }

    private void ResetView(List<T> orderedView)
    {
        _view.Clear();
        foreach (var item in orderedView)
            _view.Add(item);

        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    private void ApplyOrderedView(List<T> orderedView)
    {
        if (_view.Count != orderedView.Count)
        {
            ResetView(orderedView);
            return;
        }

        for (var targetIndex = 0; targetIndex < orderedView.Count; ++targetIndex)
        {
            var item = orderedView[targetIndex];
            var currentIndex = _view.IndexOf(item);
            if (currentIndex < 0)
            {
                ResetView(orderedView);
                return;
            }

            if (currentIndex == targetIndex)
                continue;

            _view.RemoveAt(currentIndex);
            _view.Insert(targetIndex, item);
            OnCollectionChanged(new(NotifyCollectionChangedAction.Move, item, targetIndex, currentIndex));
        }
    }

    private void SourceNcc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                AttachPropertyChangedHandler(e.NewItems);
                if (!Equals(Range, Range.All))
                {
                    HandleSourceChanged();
                    break;
                }

                if (e.NewItems is [T item])
                    _ = HandleItemAdded(e.NewStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {
                DetachPropertyChangedHandler(e.OldItems);
                if (!Equals(Range, Range.All))
                {
                    HandleSourceChanged();
                    break;
                }

                if (e.OldItems is [T item])
                    HandleItemRemoved(e.OldStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Move:
            {
                if (!Equals(Range, Range.All))
                {
                    HandleSourceChanged();
                    break;
                }

                if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0 && e.NewItems is [T item])
                    HandleItemMoved(e.OldStartingIndex, e.NewStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Replace:
            {
                DetachPropertyChangedHandler(e.OldItems);
                AttachPropertyChangedHandler(e.NewItems);
                if (!Equals(Range, Range.All))
                {
                    HandleSourceChanged();
                    break;
                }

                if (e.OldStartingIndex >= 0 && e.OldItems is [T oldItem] && e.NewItems is [T newItem])
                    HandleItemReplaced(e.OldStartingIndex, oldItem, newItem);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Reset:
            {
                HandleSourceChanged();
                break;
            }
            default:
                break;
        }
    }

    private bool HandleItemAdded(int sourceIndex, T newItem)
    {
        if (!IsSourceIndexInRange(sourceIndex) || !MatchesFilter(newItem))
            return false;

        var newViewIndex = SortDescriptions.Count is not 0
            ? GetSortedInsertIndex(newItem)
            : GetSequentialInsertIndex(sourceIndex);

        _view.Insert(newViewIndex, newItem);

        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, newViewIndex);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnCollectionChanged(e);
        return true;
    }

    private int GetSortedInsertIndex(T item)
    {
        var viewIndex = _view.BinarySearch(item, ViewComparer);
        if (viewIndex < 0)
            viewIndex = ~viewIndex;
        return viewIndex;
    }

    private int GetSequentialInsertIndex(int sourceIndex)
    {
        if (!IsReversed)
        {
            var (start, end) = GetSourceRangeBounds();
            for (int i = start, j = 0; i < end; ++i)
            {
                if (i == sourceIndex)
                    return j;

                if (MatchesFilter(Source[i]))
                    ++j;
            }

            return _view.Count;
        }

        var (rangeStart, rangeEnd) = GetSourceRangeBounds();
        for (int i = rangeEnd - 1, j = 0; i >= rangeStart; --i)
        {
            if (i == sourceIndex)
                return j;

            if (MatchesFilter(Source[i]))
                ++j;
        }

        return _view.Count;
    }

    private void HandleItemRemoved(int oldStartingIndex, T oldItem)
    {
        var viewIndex = _view.IndexOf(oldItem);
        if (viewIndex < 0)
            return;

        RemoveFromView(viewIndex, oldItem);
    }

    private void HandleItemMoved(int oldStartingIndex, int newStartingIndex, T item)
    {
        if (SortDescriptions.Count is not 0 || !IsSourceIndexInRange(newStartingIndex) || !MatchesFilter(item))
            return;

        var oldViewIndex = _view.IndexOf(item);
        if (oldViewIndex < 0)
            return;

        _view.RemoveAt(oldViewIndex);
        var newViewIndex = GetSequentialInsertIndex(newStartingIndex);
        _view.Insert(newViewIndex, item);

        if (newViewIndex != oldViewIndex)
            OnCollectionChanged(new(NotifyCollectionChangedAction.Move, item, newViewIndex, oldViewIndex));
    }

    private void HandleItemReplaced(int sourceIndex, T oldItem, T newItem)
    {
        var oldViewIndex = _view.IndexOf(oldItem);
        if (oldViewIndex < 0)
        {
            if (IsSourceIndexInRange(sourceIndex) && MatchesFilter(newItem))
                _ = HandleItemAdded(sourceIndex, newItem);

            return;
        }

        if (!IsSourceIndexInRange(sourceIndex) || !MatchesFilter(newItem))
        {
            RemoveFromView(oldViewIndex, oldItem);
            return;
        }

        _view.RemoveAt(oldViewIndex);
        var newViewIndex = SortDescriptions.Count is not 0
            ? GetSortedInsertIndex(newItem)
            : GetSequentialInsertIndex(sourceIndex);
        _view.Insert(newViewIndex, newItem);

        if (newViewIndex == oldViewIndex)
            OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, newItem, oldItem, newViewIndex));
        else
        {
            OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, oldItem, oldViewIndex));
            OnCollectionChanged(new(NotifyCollectionChangedAction.Add, newItem, newViewIndex));
        }
    }

    private void RemoveFromView(int itemIndex, T item)
    {
        _view.RemoveAt(itemIndex);
        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnCollectionChanged(e);
    }

    #region IList

    int IList.Add(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Source.Add((T)value);
        return Source.Count - 1;
    }

    bool IList.Contains(object? value) => value is T item && _view.Contains(item);

    int IList.IndexOf(object? value) => value is T item ? _view.IndexOf(item) : -1;

    void IList.Insert(int index, object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Insert(index, (T)value);
    }

    void IList.Remove(object? value)
    {
        if (value is T item)
            _ = Source.Remove(item);
    }

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_view).CopyTo(array, index);

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)Source).SyncRoot;

    bool IList.IsReadOnly => false;

    object? IList.this[int index]
    {
        get => _view[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this[index] = (T)value;
        }
    }

    bool IList.IsFixedSize => false;

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DetachSourceHandler(Source);
    }
}

file static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs(nameof(ICollection<>.Count));
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
}
