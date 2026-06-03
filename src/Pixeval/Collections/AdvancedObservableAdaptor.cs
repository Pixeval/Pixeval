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
public class AdvancedObservableAdaptor<TIn, TOut>
    : IAdvancedObservableView<TOut>
    where TIn : class
    where TOut : class
{
    private readonly Func<TIn, TOut> _factory;

    private readonly AdvancedObservableCollection<TOut> _inner;

    private ObservableCollection<TIn>? _source;

    public AdvancedObservableAdaptor(Func<TIn, TOut> factory) : this([], factory)
    {
    }

    public AdvancedObservableAdaptor(ObservableCollection<TIn> source, Func<TIn, TOut> factory, bool isLiveShaping = false)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _inner = new([], isLiveShaping);
        _inner.CollectionChanged += InnerOnCollectionChanged;
        _inner.PropertyChanged += InnerOnPropertyChanged;
        _inner.FilterChanged += InnerOnFilterChanged;
        Source = source;
    }

    public ObservableCollection<TIn> Source
    {
        get => _source!;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(_source, value))
                return;

            DetachSourceHandler(_source);
            _source = value;
            AttachSourceHandler(value);
            RebuildMappedSource();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TOut> MappedSource { get; private set; } = [];

    public bool IsReversed
    {
        get => _inner.IsReversed;
        set => _inner.IsReversed = value;
    }

    public ObservableCollection<ISortDescription<TOut>> SortDescriptions => _inner.SortDescriptions;

    public ObservableCollection<IFilter<TOut>> Filters => _inner.Filters;

    public FilterCombinationMode FilterCombinationMode
    {
        get => _inner.FilterCombinationMode;
        set => _inner.FilterCombinationMode = value;
    }

    public SimpleDefer<TOut> DeferSortDescriptionsChange() => _inner.DeferSortDescriptionsChange();

    public SimpleDefer<TOut> DeferFiltersChange() => _inner.DeferFiltersChange();

    public IEnumerator<TOut> GetEnumerator() => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TIn item) => Source.Add(item);

    public void Clear() => Source.Clear();

    public bool Contains(TOut item) => _inner.Contains(item);

    public void CopyTo(TOut[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

    public bool Remove(TIn item) => Source.Remove(item);

    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    public int IndexOf(TOut item) => _inner.IndexOf(item);

    public void Insert(int index, TIn item)
    {
        var sourceIndex = index < _inner.Count ? MappedSource.IndexOf(_inner[index]) : Source.Count;
        Source.Insert(sourceIndex, item);
    }

    public void RemoveAt(int index)
    {
        var mappedIndex = MappedSource.IndexOf(_inner[index]);
        if (mappedIndex >= 0)
            Source.RemoveAt(mappedIndex);
    }

    public TOut this[int index] => _inner[index];

    public Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default)
    {
        return (Source as IIncrementalLoading)?.LoadMoreItemsAsync(count, token) ?? Task.FromResult(0);
    }

    public bool HasMoreItems => (Source as IIncrementalLoading)?.HasMoreItems ?? false;

    public Range Range
    {
        get => _inner.Range;
        set => _inner.Range = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event EventHandler<AdvancedObservableAdaptor<TIn, TOut>, EventArgs>? FilterChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    private void InnerOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnCollectionChanged(e);
    }

    private void InnerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdvancedObservableCollection<TOut>.Source))
            return;

        OnPropertyChanged(e);
    }

    private void InnerOnFilterChanged(AdvancedObservableCollection<TOut> sender, EventArgs e)
    {
        FilterChanged?.Invoke(this, e);
    }

    private void RebuildMappedSource()
    {
        MappedSource = new ObservableCollection<TOut>(Source.Select(_factory));
        _inner.Source = MappedSource;
    }

    private void AttachSourceHandler(ObservableCollection<TIn>? items)
    {
        if (items is not null)
            items.CollectionChanged += SourceOnCollectionChanged;
    }

    private void DetachSourceHandler(ObservableCollection<TIn>? items)
    {
        if (items is not null)
            items.CollectionChanged -= SourceOnCollectionChanged;
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e is { NewItems: { } newItems, NewStartingIndex: >= 0 }:
            {
                var insertIndex = e.NewStartingIndex;
                foreach (TIn item in newItems)
                    MappedSource.Insert(insertIndex++, _factory(item));

                break;
            }
            case NotifyCollectionChangedAction.Remove when e is { OldItems.Count: > 0, OldStartingIndex: >= 0 }:
            {
                for (var index = 0; index < e.OldItems.Count; ++index)
                    MappedSource.RemoveAt(e.OldStartingIndex);

                break;
            }
            case NotifyCollectionChangedAction.Move when e is { OldStartingIndex: >= 0, NewStartingIndex: >= 0 }:
            {
                if (e.NewItems is not { Count: 1 })
                {
                    RebuildMappedSource();
                    break;
                }

                MappedSource.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;
            }
            case NotifyCollectionChangedAction.Replace when e is { NewItems: { } replacementItems, NewStartingIndex: >= 0 }:
            {
                for (var index = 0; index < replacementItems.Count; ++index)
                    MappedSource[e.NewStartingIndex + index] = _factory((TIn)replacementItems[index]!);

                break;
            }
            case NotifyCollectionChangedAction.Reset:
            default:
                RebuildMappedSource();
                break;
        }
    }

    int IList.Add(object? value) => throw new NotSupportedException();

    bool IList.Contains(object? value) => value is TOut item && _inner.Contains(item);

    int IList.IndexOf(object? value) => value is TOut item ? _inner.IndexOf(item) : -1;

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    int ICollection.Count => _inner.Count;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_inner).CopyTo(array, index);

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)_inner).SyncRoot;

    bool IList.IsReadOnly => true;

    object? IList.this[int index]
    {
        get => _inner[index];
        set => throw new NotSupportedException();
    }

    bool IList.IsFixedSize => true;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _inner.FilterChanged -= InnerOnFilterChanged;
        _inner.PropertyChanged -= InnerOnPropertyChanged;
        _inner.CollectionChanged -= InnerOnCollectionChanged;
        DetachSourceHandler(_source);
        _inner.Dispose();
    }
}
