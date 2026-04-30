// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Collections;

[DebuggerDisplay("Count = {Count}")]
public class AdvancedObservableCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
    : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading, IComparer<T>, IDisposable where T : class
{
    private readonly bool _liveShapingEnabled;

    private readonly HashSet<string> _observedFilterProperties = [];

    private readonly List<T> _view = [];

    private IList ListView => RangedView;

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
        SortDescriptions.CollectionChanged += SortDescriptionsCollectionChanged;
        Source = source;
        return;

        void SortDescriptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => HandleSortChanged();
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

    #region IList<T>

    private List<T> RangedView
    {
        get
        {
            if (Equals(Range, Range.All))
                return _view;
            var viewCount = _view.Count;
            var start = Range.Start.GetOffset(viewCount);
            if (start > viewCount)
                return [];
            var end = Range.End.GetOffset(viewCount);
            if (end < 0)
                return [];
            if (start > end)
                return [];
            start = Math.Max(0, start);
            end = Math.Min(viewCount, end);
            return _view[start..end];
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => RangedView.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => RangedView.GetEnumerator();

    /// <inheritdoc />
    public void Add(T item) => Source.Add(item);

    /// <inheritdoc cref="ICollection{T}.Clear"/> />
    public void Clear() => Source.Clear();

    /// <inheritdoc />
    public bool Contains(T item) => RangedView.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => RangedView.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(T item) => Source.Remove(item);

    /// <inheritdoc cref="ICollection{T}.Count"/> />
    public int Count => RangedView.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int IndexOf(T item) => RangedView.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, T item) => Source.Insert(index, item);

    /// <inheritdoc cref="IList{T}.RemoveAt"/> />
    public void RemoveAt(int index) => Source.Remove(RangedView[index]);

    /// <inheritdoc cref="List{T}.this[int]"/>
    public T this[int index]
    {
        get => RangedView[index];
        set => RangedView[index] = value;
    }

    #endregion

    /// <inheritdoc cref="ISupportIncrementalLoading.LoadMoreItemsAsync"/>
    public Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default) => (Source as ISupportIncrementalLoading)?.LoadMoreItemsAsync(count, token) ?? Task.FromResult(0);

    /// <inheritdoc cref="ISupportIncrementalLoading.HasMoreItems"/>
    public bool HasMoreItems => (Source as ISupportIncrementalLoading)?.HasMoreItems ?? false;

    /// <summary>
    /// Gets or sets the predicate used to filter the visible items
    /// </summary>
    public Func<T, bool>? Filter
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            RaiseFilterChanged();
        }
    }

    public Range Range
    {
        get;
        set
        {
            if (field.Equals(value))
                return;

            field = value;
            OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        }
    } = Range.All;

    /// <summary>
    /// Gets SortDescriptions to sort the visible items
    /// </summary>
    public ObservableCollection<ISortDescription<T>> SortDescriptions { get; } = [];

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    int IComparer<T>.Compare(T? x, T? y)
    {
        foreach (var sd in SortDescriptions)
            if (sd.Comparison(x, y) is var cmp and not 0)
                return sd.IsDescending ? -cmp : +cmp;

        return 0;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when the <see cref="Filter"/> changes.
    /// </summary>
    public event EventHandler<AdvancedObservableCollection<T>, Func<T, bool>?>? FilterChanged;

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

    /// <summary>
    /// Add a property to re-filter an item on when it is changed
    /// </summary>
    public void ObserveFilterProperty(string propertyName)
    {
        _ = _observedFilterProperties.Add(propertyName);
    }

    /// <summary>
    /// Remove a property to re-filter an item on when it is changed
    /// </summary>
    public void UnobserveFilterProperty(string propertyName)
    {
        _ = _observedFilterProperties.Remove(propertyName);
    }

    /// <summary>
    /// Clears all properties items are re-filtered on
    /// </summary>
    public void ClearObservedFilterProperties()
    {
        _observedFilterProperties.Clear();
    }

    private void ItemOnPropertyChanged(object? i, PropertyChangedEventArgs e)
    {
        if (!_liveShapingEnabled || i is not T item)
            return;

        var filterResult = Filter?.Invoke(item);

        if (filterResult.HasValue && _observedFilterProperties.Contains(e.PropertyName!))
        {
            var viewIndex = _view.IndexOf(item);
            if (viewIndex is not -1 && !filterResult.Value)
                RemoveFromView(viewIndex, item);
            else if (viewIndex is -1 && filterResult.Value)
            {
                var index = Source.IndexOf(item);
                _ = HandleItemAdded(index, item);
            }
        }

        if ((filterResult ?? true) && (SortDescriptions.Count is 0 || SortDescriptions.Any(sd => sd.ObservedProperties.Contains(e.PropertyName!))))
        {
            var oldIndex = _view.IndexOf(item);

            // Check if item is in view:
            if (oldIndex < 0)
                return;

            _view.RemoveAt(oldIndex);
            var targetIndex = _view.BinarySearch(item, this);
            if (targetIndex < 0)
                targetIndex = ~targetIndex;

            _view.Insert(targetIndex, item);

            // Only trigger expensive UI updates if the index really changed:
            if (targetIndex != oldIndex)
                OnCollectionChanged(new(NotifyCollectionChangedAction.Move, item, targetIndex, oldIndex));
        }
        else if (string.IsNullOrEmpty(e.PropertyName))
            HandleSourceChanged();
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

    private void HandleSortChanged()
    {
        if (SortDescriptions.Count is 0)
        {
            var newIndex = 0;
            foreach (var item in Source)
            {
                if (_view.IndexOf(item) is var index and not -1)
                    // 元素重复时可能出现index < newIndex
                    if (index == newIndex)
                        ++newIndex;
                    else if (index > newIndex)
                    {
                        _view.RemoveAt(index);
                        _view.Insert(newIndex, item);
                        ++newIndex;
                    }
            }
        }
        else
            _view.Sort(this);

        OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
    }

    public void RaiseFilterChanged()
    {
        if (Filter is not null)
        {
            for (var index = 0; index < _view.Count; ++index)
            {
                var item = _view[index];
                if (Filter(item))
                    continue;

                RemoveFromView(index, item);
                index--;
            }
        }

        var viewHash = this.ToFrozenSet();
        var viewIndex = 0;
        for (var index = 0; index < Source.Count; ++index)
        {
            var item = Source[index];
            if (viewHash.Contains(item))
            {
                ++viewIndex;
                continue;
            }

            if (HandleItemAdded(index, item, viewIndex))
                ++viewIndex;
        }

        FilterChanged?.Invoke(this, Filter);
    }

    private void HandleSourceChanged()
    {
        _view.Clear();
        foreach (var item in Source)
        {
            if (Filter is not null && !Filter(item))
                continue;

            if (SortDescriptions.Count is not 0)
            {
                var targetIndex = _view.BinarySearch(item, this);
                if (targetIndex < 0)
                    targetIndex = ~targetIndex;

                _view.Insert(targetIndex, item);
            }
            else
            {
                _view.Add(item);
            }
        }

        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    private void SourceNcc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                AttachPropertyChangedHandler(e.NewItems);
                if (e.NewItems is [T item])
                    _ = HandleItemAdded(e.NewStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {
                DetachPropertyChangedHandler(e.OldItems);
                if (e.OldItems is [T item])
                    HandleItemRemoved(e.OldStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
            {
                HandleSourceChanged();
                break;
            }
            default:
                break;
        }
    }

    private bool HandleItemAdded(int sourceIndex, T newItem, int? viewIndex = null)
    {
        if (Filter is not null && !Filter(newItem))
            return false;

        var newViewIndex = _view.Count;

        if (SortDescriptions.Count is not 0)
        {
            newViewIndex = _view.BinarySearch(newItem, this);
            if (newViewIndex < 0)
                newViewIndex = ~newViewIndex;
        }
        else if (sourceIndex is 0 || _view.Count is 0)
            newViewIndex = 0;
        else if (viewIndex.HasValue)
            newViewIndex = viewIndex.Value;
        else if (_view.Count == Source.Count - 1)
            newViewIndex = _view.Count;
        else
        {
            for (int i = 0, j = 0; i < Source.Count; ++i)
            {
                if (i == sourceIndex || j >= _view.Count)
                {
                    newViewIndex = j;
                    break;
                }

                if (_view[j] == Source[i])
                    j++;
            }
        }

        _view.Insert(newViewIndex, newItem);

        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, newViewIndex);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnCollectionChanged(e);
        return true;
    }

    private void HandleItemRemoved(int oldStartingIndex, T oldItem)
    {
        if (Filter is not null && !Filter(oldItem))
            return;

        if (oldStartingIndex < 0 || oldStartingIndex >= _view.Count || !Equals(_view[oldStartingIndex], oldItem))
            oldStartingIndex = _view.IndexOf(oldItem);

        if (oldStartingIndex < 0)
            return;
        RemoveFromView(oldStartingIndex, oldItem);
    }

    private void RemoveFromView(int itemIndex, T item)
    {
        _view.RemoveAt(itemIndex);
        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnCollectionChanged(e);
    }

    #region IList

    int IList.Add(object? value) => ListView.Add(value);

    bool IList.Contains(object? value) => ListView.Contains(value);

    int IList.IndexOf(object? value) => ListView.IndexOf(value);

    void IList.Insert(int index, object? value) => ListView.Insert(index, value);

    void IList.Remove(object? value) => ListView.Remove(value);

    void ICollection.CopyTo(Array array, int index) => ListView.CopyTo(array, index);

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ListView.SyncRoot;

    bool IList.IsReadOnly => false;

    object? IList.this[int index]
    {
        get => ListView[index];
        set => ListView[index] = value;
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
