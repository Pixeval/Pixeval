// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

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
public class AdvancedObservableAdaptor<TIn, TOut>
    : DeferSortDescriptions<TOut>, IList, IReadOnlyList<TOut>, INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading, IComparer<TOut>, IDisposable
    where TIn : class
    where TOut : class
{
    private readonly Func<TIn, TOut> _factory;

    private readonly bool _liveShapingEnabled;

    private readonly HashSet<string> _observedFilterProperties = [];

    private readonly List<TOut> _mapped = [];

    private readonly List<TOut> _view = [];

    private ObservableCollection<TIn>? _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedObservableAdaptor{TIn, TOut}"/> class.
    /// </summary>
    public AdvancedObservableAdaptor(Func<TIn, TOut> factory) : this([], factory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedObservableAdaptor{TIn, TOut}"/> class.
    /// </summary>
    /// <param name="source">source IEnumerable</param>
    /// <param name="factory">factory to build view item from source item</param>
    /// <param name="isLiveShaping">Denotes whether this adaptor should re-filter/re-sort if a PropertyChanged is raised for an observed property.</param>
    public AdvancedObservableAdaptor(ObservableCollection<TIn> source, Func<TIn, TOut> factory, bool isLiveShaping = false)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _liveShapingEnabled = isLiveShaping;
        Source = source;
    }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public ObservableCollection<TIn> Source
    {
        get => _source ?? [];
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(_source, value))
                return;

            DetachSourceHandler(_source);
            _source = value;
            AttachSourceHandler(_source);

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

    private List<TOut> RangedView
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
    public IEnumerator<TOut> GetEnumerator() => RangedView.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TIn item) => Source.Add(item);

    /// <inheritdoc cref="ICollection{T}.Clear"/>
    public void Clear() => Source.Clear();

    /// <summary>
    /// Determines whether the view contains the provided item.
    /// </summary>
    public bool Contains(TOut item) => RangedView.Contains(item);

    /// <summary>
    /// Copies the view into the target array.
    /// </summary>
    public void CopyTo(TOut[] array, int arrayIndex) => RangedView.CopyTo(array, arrayIndex);

    public bool Remove(TIn item) => Source.Remove(item);

    /// <summary>
    /// Gets the number of items in the view.
    /// </summary>
    public int Count => RangedView.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Returns the index of the item in the view.
    /// </summary>
    public int IndexOf(TOut item) => RangedView.IndexOf(item);

    public void Insert(int index, TIn item) => Source.Insert(index, item);

    /// <inheritdoc cref="IList{T}.RemoveAt"/>
    public void RemoveAt(int index) => Source.RemoveAt(index);

    /// <summary>
    /// Gets the item in the current view.
    /// </summary>
    public TOut this[int index] => RangedView[index];

    /// <inheritdoc cref="ISupportIncrementalLoading.LoadMoreItemsAsync"/>
    public Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default) => (Source as ISupportIncrementalLoading)?.LoadMoreItemsAsync(count, token) ?? Task.FromResult(0);

    /// <inheritdoc cref="ISupportIncrementalLoading.HasMoreItems"/>
    public bool HasMoreItems => (Source as ISupportIncrementalLoading)?.HasMoreItems ?? false;

    /// <summary>
    /// Gets or sets the predicate used to filter the visible items.
    /// </summary>
    public Func<TOut, bool>? Filter
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

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    int IComparer<TOut>.Compare(TOut? x, TOut? y) => CompareCore(x, y);

    private int CompareCore(TOut? x, TOut? y)
    {
        foreach (var sd in SortDescriptions)
            if (sd.Comparison(x, y) is var cmp and not 0)
                return sd.IsDescending ? -cmp : +cmp;

        return 0;
    }

    private int CompareView(TOut? x, TOut? y)
    {
        var comparison = CompareCore(x, y);
        return IsReversed ? -comparison : comparison;
    }

    private IComparer<TOut> ViewComparer => Comparer<TOut>.Create(CompareView);

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
    public event EventHandler<AdvancedObservableAdaptor<TIn, TOut>, Func<TOut, bool>?>? FilterChanged;

    /// <summary>
    /// Property changed event invoker.
    /// </summary>
    /// <param name="propertyName">name of the property that changed</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Property changed event invoker.
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    /// <summary>
    /// Add a property to re-filter an item on when it is changed.
    /// </summary>
    public void ObserveFilterProperty(string propertyName)
    {
        _ = _observedFilterProperties.Add(propertyName);
    }

    /// <summary>
    /// Remove a property to re-filter an item on when it is changed.
    /// </summary>
    public void UnobserveFilterProperty(string propertyName)
    {
        _ = _observedFilterProperties.Remove(propertyName);
    }

    /// <summary>
    /// Clears all properties items are re-filtered on.
    /// </summary>
    public void ClearObservedFilterProperties()
    {
        _observedFilterProperties.Clear();
    }

    private bool MatchesFilter(TOut item) => Filter?.Invoke(item) ?? true;

    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_liveShapingEnabled)
            return;

        if (string.IsNullOrEmpty(e.PropertyName))
        {
            HandleSourceChanged();
            return;
        }

        for (var sourceIndex = 0; sourceIndex < _mapped.Count; ++sourceIndex)
        {
            var sourceItem = Source[sourceIndex];
            var viewItem = _mapped[sourceIndex];
            if (!ReferenceEquals(viewItem, sender) && !ReferenceEquals(sourceItem, sender))
                continue;

            var filterResult = Filter?.Invoke(viewItem);

            if (filterResult.HasValue && _observedFilterProperties.Contains(e.PropertyName))
            {
                var viewIndex = _view.IndexOf(viewItem);
                if (viewIndex is not -1 && !filterResult.Value)
                    RemoveFromView(viewIndex, viewItem);
                else if (viewIndex is -1 && filterResult.Value)
                    _ = InsertIntoView(sourceIndex, viewItem);
            }

            if ((filterResult ?? true)
                && SortDescriptions.Count is not 0
                && SortDescriptions.Any(sd => sd.ObservedProperties.Contains(e.PropertyName)))
            {
                var oldIndex = _view.IndexOf(viewItem);
                if (oldIndex < 0)
                    continue;

                _view.RemoveAt(oldIndex);
                var targetIndex = GetSortedInsertIndex(viewItem);

                _view.Insert(targetIndex, viewItem);

                if (targetIndex != oldIndex)
                    OnCollectionChanged(new(NotifyCollectionChangedAction.Move, viewItem, targetIndex, oldIndex));
            }
        }
    }

    private void AttachSourceHandler(ObservableCollection<TIn>? items)
    {
        if (items is null)
            return;

        items.CollectionChanged += SourceNcc_CollectionChanged;
    }

    private void DetachSourceHandler(ObservableCollection<TIn>? items)
    {
        if (items is not null)
            items.CollectionChanged -= SourceNcc_CollectionChanged;

        DetachPropertyChangedHandlers();
    }

    private void AttachPropertyChangedHandler(TIn sourceItem, TOut viewItem)
    {
        if (!_liveShapingEnabled)
            return;

        if (sourceItem is INotifyPropertyChanged sourceNotify)
            sourceNotify.PropertyChanged += ItemOnPropertyChanged;

        if (!ReferenceEquals(sourceItem, viewItem) && viewItem is INotifyPropertyChanged viewNotify)
            viewNotify.PropertyChanged += ItemOnPropertyChanged;
    }

    private void DetachPropertyChangedHandlers()
    {
        if (!_liveShapingEnabled)
            return;

        for (var i = 0; i < _mapped.Count && i < Source.Count; ++i)
            DetachPropertyChangedHandler(Source[i], _mapped[i]);
    }

    private void DetachPropertyChangedHandler(TIn sourceItem, TOut viewItem)
    {
        if (!_liveShapingEnabled)
            return;

        if (sourceItem is INotifyPropertyChanged sourceNotify)
            sourceNotify.PropertyChanged -= ItemOnPropertyChanged;

        if (!ReferenceEquals(sourceItem, viewItem) && viewItem is INotifyPropertyChanged viewNotify)
            viewNotify.PropertyChanged -= ItemOnPropertyChanged;
    }

    protected override void SortChangedOverride()
    {
        ApplyOrderedView(CreateOrderedVisibleView());
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

        var remaining = new List<TOut>(_view);
        for (var index = 0; index < _mapped.Count; ++index)
        {
            var item = _mapped[index];
            if (!MatchesFilter(item))
                continue;

            if (remaining.IndexOf(item) is var existingIndex and >= 0)
            {
                remaining.RemoveAt(existingIndex);
                continue;
            }

            _ = InsertIntoView(index, item);
        }

        FilterChanged?.Invoke(this, Filter);
    }

    private void HandleSourceChanged()
    {
        DetachPropertyChangedHandlers();
        _mapped.Clear();

        foreach (var item in Source)
        {
            var viewItem = _factory(item);
            _mapped.Add(viewItem);
            AttachPropertyChangedHandler(item, viewItem);
        }

        ResetView(CreateOrderedVisibleView());
    }

    private List<TOut> CreateOrderedVisibleView()
    {
        var orderedView = new List<TOut>();
        foreach (var item in _mapped.Where(MatchesFilter))
        {
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

    private void ResetView(List<TOut> orderedView)
    {
        _view.Clear();
        foreach (var item in orderedView)
            _view.Add(item);

        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    private void ApplyOrderedView(List<TOut> orderedView)
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
                if (e is { NewStartingIndex: >= 0, NewItems: [TIn item] })
                    _ = HandleItemAdded(e.NewStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {
                if (e is { OldStartingIndex: >= 0, OldItems: [TIn item] })
                    HandleItemRemoved(e.OldStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Move:
            {
                if (e is { OldStartingIndex: >= 0, NewStartingIndex: >= 0, NewItems: [TIn item] })
                    HandleItemMoved(e.OldStartingIndex, e.NewStartingIndex, item);
                else
                    HandleSourceChanged();

                break;
            }
            case NotifyCollectionChangedAction.Replace:
            {
                if (e is { OldStartingIndex: >= 0, NewStartingIndex: >= 0, OldItems: [TIn oldItem], NewItems: [TIn newItem] })
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

    private bool HandleItemAdded(int sourceIndex, TIn newItem)
    {
        var viewItem = _factory(newItem);
        _mapped.Insert(sourceIndex, viewItem);
        AttachPropertyChangedHandler(newItem, viewItem);
        return InsertIntoView(sourceIndex, viewItem);
    }

    private bool InsertIntoView(int sourceIndex, TOut newItem)
    {
        if (!MatchesFilter(newItem))
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

    private int GetSortedInsertIndex(TOut item)
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
            for (int i = 0, j = 0; i < _mapped.Count; ++i)
            {
                if (i == sourceIndex || j >= _view.Count)
                    return j;

                if (MatchesFilter(_mapped[i]))
                    ++j;
            }

            return _view.Count;
        }

        for (int i = _mapped.Count - 1, j = 0; i >= 0; --i)
        {
            if (i == sourceIndex || j >= _view.Count)
                return j;

            if (MatchesFilter(_mapped[i]))
                ++j;
        }

        return _view.Count;
    }

    private void HandleItemRemoved(int oldStartingIndex, TIn oldItem)
    {
        if (oldStartingIndex < 0 || oldStartingIndex >= _mapped.Count)
        {
            HandleSourceChanged();
            return;
        }

        var viewItem = _mapped[oldStartingIndex];
        DetachPropertyChangedHandler(oldItem, viewItem);
        _mapped.RemoveAt(oldStartingIndex);

        var viewIndex = _view.IndexOf(viewItem);
        if (viewIndex < 0)
            return;

        RemoveFromView(viewIndex, viewItem);
    }

    private void HandleItemMoved(int oldStartingIndex, int newStartingIndex, TIn item)
    {
        if (oldStartingIndex < 0 || oldStartingIndex >= _mapped.Count || newStartingIndex < 0 || newStartingIndex > _mapped.Count)
        {
            HandleSourceChanged();
            return;
        }

        var viewItem = _mapped[oldStartingIndex];
        _mapped.RemoveAt(oldStartingIndex);
        _mapped.Insert(newStartingIndex, viewItem);

        if (SortDescriptions.Count is not 0 || !MatchesFilter(viewItem))
            return;

        var oldViewIndex = _view.IndexOf(viewItem);
        if (oldViewIndex < 0)
            return;

        _view.RemoveAt(oldViewIndex);
        var newViewIndex = GetSequentialInsertIndex(newStartingIndex);
        _view.Insert(newViewIndex, viewItem);

        if (newViewIndex != oldViewIndex)
            OnCollectionChanged(new(NotifyCollectionChangedAction.Move, viewItem, newViewIndex, oldViewIndex));
    }

    private void HandleItemReplaced(int sourceIndex, TIn oldItem, TIn newItem)
    {
        if (sourceIndex < 0 || sourceIndex >= _mapped.Count)
        {
            HandleSourceChanged();
            return;
        }

        var oldViewItem = _mapped[sourceIndex];
        DetachPropertyChangedHandler(oldItem, oldViewItem);

        var newViewItem = _factory(newItem);
        _mapped[sourceIndex] = newViewItem;
        AttachPropertyChangedHandler(newItem, newViewItem);

        var oldViewIndex = _view.IndexOf(oldViewItem);
        if (oldViewIndex < 0)
        {
            if (MatchesFilter(newViewItem))
                _ = InsertIntoView(sourceIndex, newViewItem);

            return;
        }

        if (!MatchesFilter(newViewItem))
        {
            RemoveFromView(oldViewIndex, oldViewItem);
            return;
        }

        _view.RemoveAt(oldViewIndex);
        var newViewIndex = SortDescriptions.Count is not 0
            ? GetSortedInsertIndex(newViewItem)
            : GetSequentialInsertIndex(sourceIndex);
        _view.Insert(newViewIndex, newViewItem);

        if (newViewIndex == oldViewIndex)
            OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, newViewItem, oldViewItem, newViewIndex));
        else
        {
            OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, oldViewItem, oldViewIndex));
            OnCollectionChanged(new(NotifyCollectionChangedAction.Add, newViewItem, newViewIndex));
        }
    }

    private void RemoveFromView(int itemIndex, TOut item)
    {
        _view.RemoveAt(itemIndex);
        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnCollectionChanged(e);
    }

    int IList.Add(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Source.Add((TIn)value);
        return Source.Count - 1;
    }

    bool IList.Contains(object? value) => value is TIn item && Source.Contains(item);

    int IList.IndexOf(object? value) => value is TIn item ? Source.IndexOf(item) : -1;

    void IList.Insert(int index, object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Source.Insert(index, (TIn)value);
    }

    void IList.Remove(object? value)
    {
        if (value is TIn item)
            _ = Source.Remove(item);
    }

    int ICollection.Count => Source.Count;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)Source).CopyTo(array, index);

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)Source).SyncRoot;

    bool IList.IsReadOnly => false;

    object? IList.this[int index]
    {
        get => Source[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Source[index] = (TIn)value;
        }
    }

    bool IList.IsFixedSize => false;

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DetachSourceHandler(_source);
    }
}

file static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(ICollection<>.Count));
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
}
