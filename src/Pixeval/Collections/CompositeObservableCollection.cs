// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Pixeval.Utilities;

namespace Pixeval.Collections;

/// <summary>
/// Exposes several collections as one read-only, flattened collection.
/// </summary>
/// <typeparam name="T">The common item type exposed by the composite collection.</typeparam>
public sealed class CompositeObservableCollection<T>
    : IReadOnlyList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
    private readonly List<T> _items = [];

    private readonly List<SourceCollection> _sources = [];

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeObservableCollection{T}"/> class.
    /// </summary>
    /// <param name="sources">The collections to flatten in the specified order.</param>
    public CompositeObservableCollection(params IEnumerable<IEnumerable<T>> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        foreach (var source in sources)
            AddSourceCore(source, false);
    }

    /// <summary>
    /// Gets the number of items in the composite collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets the item at the specified flattened index.
    /// </summary>
    public T this[int index] => _items[index];

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Adds a collection to the end of the composite collection.
    /// </summary>
    public void AddSource(IEnumerable<T> source)
    {
        AddSourceCore(source, true);
    }

    /// <summary>
    /// Removes a collection from the composite collection.
    /// </summary>
    /// <returns><see langword="true"/> if the source was removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveSource(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var sourceIndex = _sources.FindIndex(item => ReferenceEquals(item.Source, source));
        if (sourceIndex < 0)
            return false;

        var sourceCollection = _sources[sourceIndex];
        var compositeIndex = GetCompositeIndex(sourceIndex);
        var removedItems = sourceCollection.Items.ToArray();

        sourceCollection.Listener?.Detach();
        _sources.RemoveAt(sourceIndex);
        _items.RemoveRange(compositeIndex, removedItems.Length);

        if (removedItems.Length is not 0)
        {
            OnPropertyChanged(CompositeObservableCollectionEventArgs.CountChanged);
            OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                removedItems,
                compositeIndex));
        }

        return true;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        foreach (var source in _sources)
            source.Listener?.Detach();

        _sources.Clear();
        CollectionChanged = null;
        PropertyChanged = null;
        GC.SuppressFinalize(this);
    }

    private void AddSourceCore(IEnumerable<T> source, bool notify)
    {
        ArgumentNullException.ThrowIfNull(source);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_sources.Any(item => ReferenceEquals(item.Source, source)))
            throw new ArgumentException("The source has already been added.", nameof(source));

        var sourceCollection = new SourceCollection(source, [.. source]);
        var compositeIndex = _items.Count;
        _sources.Add(sourceCollection);
        _items.AddRange(sourceCollection.Items);

        if (source is INotifyCollectionChanged notifyingSource)
        {
            var listener = new WeakEventListener<CompositeObservableCollection<T>, object?, NotifyCollectionChangedEventArgs>(this)
            {
                OnEventAction = static (collection, sender, args) => collection.SourceCollectionChanged(sender, args),
                OnDetachAction = weakListener => notifyingSource.CollectionChanged -= weakListener.OnEvent
            };
            sourceCollection.Listener = listener;
            notifyingSource.CollectionChanged += listener.OnEvent;
        }

        if (notify && sourceCollection.Items.Count is not 0)
        {
            OnPropertyChanged(CompositeObservableCollectionEventArgs.CountChanged);
            OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                sourceCollection.Items.ToArray(),
                compositeIndex));
        }
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not INotifyCollectionChanged source)
            return;

        var sourceCollection = _sources.FirstOrDefault(item => ReferenceEquals(item.Source, source));
        if (sourceCollection is null)
            return;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                HandleAdd(sourceCollection, args);
                break;
            case NotifyCollectionChangedAction.Remove:
                HandleRemove(sourceCollection, args);
                break;
            case NotifyCollectionChangedAction.Replace:
                HandleReplace(sourceCollection, args);
                break;
            case NotifyCollectionChangedAction.Move:
                HandleMove(sourceCollection, args);
                break;
            case NotifyCollectionChangedAction.Reset:
            default:
                ResetSource(sourceCollection);
                break;
        }
    }

    private void HandleAdd(SourceCollection source, NotifyCollectionChangedEventArgs args)
    {
        if (args.NewItems is not { Count: > 0 }
            || args.NewStartingIndex < 0
            || args.NewStartingIndex > source.Items.Count)
        {
            ResetSource(source);
            return;
        }

        var addedItems = args.NewItems.Cast<T>().ToList();
        var compositeIndex = GetCompositeIndex(source) + args.NewStartingIndex;
        source.Items.InsertRange(args.NewStartingIndex, addedItems);
        _items.InsertRange(compositeIndex, addedItems);

        OnPropertyChanged(CompositeObservableCollectionEventArgs.CountChanged);
        OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            addedItems,
            compositeIndex));
    }

    private void HandleRemove(SourceCollection source, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not { Count: > 0 }
            || args.OldStartingIndex < 0
            || args.OldStartingIndex + args.OldItems.Count > source.Items.Count)
        {
            ResetSource(source);
            return;
        }

        var compositeIndex = GetCompositeIndex(source) + args.OldStartingIndex;
        var removedItems = source.Items.GetRange(args.OldStartingIndex, args.OldItems.Count);
        source.Items.RemoveRange(args.OldStartingIndex, args.OldItems.Count);
        _items.RemoveRange(compositeIndex, removedItems.Count);

        OnPropertyChanged(CompositeObservableCollectionEventArgs.CountChanged);
        OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            removedItems,
            compositeIndex));
    }

    private void HandleReplace(SourceCollection source, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not { Count: > 0 }
            || args.NewItems is not { Count: > 0 }
            || args.OldItems.Count != args.NewItems.Count
            || args.NewStartingIndex < 0
            || args.NewStartingIndex + args.OldItems.Count > source.Items.Count)
        {
            ResetSource(source);
            return;
        }

        var compositeIndex = GetCompositeIndex(source) + args.NewStartingIndex;
        var oldItems = source.Items.GetRange(args.NewStartingIndex, args.OldItems.Count);
        var replacementItems = args.NewItems.Cast<T>().ToList();
        source.Items.RemoveRange(args.NewStartingIndex, oldItems.Count);
        source.Items.InsertRange(args.NewStartingIndex, replacementItems);
        _items.RemoveRange(compositeIndex, oldItems.Count);
        _items.InsertRange(compositeIndex, replacementItems);

        OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace,
            replacementItems,
            oldItems,
            compositeIndex));
    }

    private void HandleMove(SourceCollection source, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not { Count: > 0 }
            || args.OldStartingIndex < 0
            || args.NewStartingIndex < 0
            || args.OldStartingIndex + args.OldItems.Count > source.Items.Count
            || args.NewStartingIndex > source.Items.Count - args.OldItems.Count)
        {
            ResetSource(source);
            return;
        }

        var compositeIndex = GetCompositeIndex(source);
        var movedItems = source.Items.GetRange(args.OldStartingIndex, args.OldItems.Count);
        source.Items.RemoveRange(args.OldStartingIndex, movedItems.Count);
        source.Items.InsertRange(args.NewStartingIndex, movedItems);

        var oldCompositeIndex = compositeIndex + args.OldStartingIndex;
        var newCompositeIndex = compositeIndex + args.NewStartingIndex;
        _items.RemoveRange(oldCompositeIndex, movedItems.Count);
        _items.InsertRange(newCompositeIndex, movedItems);

        OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move,
            movedItems,
            newCompositeIndex,
            oldCompositeIndex));
    }

    private void ResetSource(SourceCollection source)
    {
        var compositeIndex = GetCompositeIndex(source);
        var oldCount = source.Items.Count;
        var currentItems = source.Source.ToList();
        source.Items.Clear();
        source.Items.AddRange(currentItems);
        _items.RemoveRange(compositeIndex, oldCount);
        _items.InsertRange(compositeIndex, currentItems);

        if (oldCount != currentItems.Count)
            OnPropertyChanged(CompositeObservableCollectionEventArgs.CountChanged);

        OnPropertyChanged(CompositeObservableCollectionEventArgs.IndexerChanged);
        OnCollectionChanged(CompositeObservableCollectionEventArgs.ResetChanged);
    }

    private int GetCompositeIndex(SourceCollection source)
    {
        var sourceIndex = _sources.IndexOf(source);
        if (sourceIndex < 0)
            throw new InvalidOperationException("The source is not part of this composite collection.");

        return GetCompositeIndex(sourceIndex);
    }

    private int GetCompositeIndex(int sourceIndex)
    {
        var compositeIndex = 0;
        for (var index = 0; index < sourceIndex; ++index)
            compositeIndex += _sources[index].Items.Count;

        return compositeIndex;
    }

    private void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);

    private sealed class SourceCollection(IEnumerable<T> source, List<T> items)
    {
        public IEnumerable<T> Source { get; } = source;

        public List<T> Items { get; } = items;

        public WeakEventListener<CompositeObservableCollection<T>, object?, NotifyCollectionChangedEventArgs>? Listener { get; set; }
    }

    #region IList

    bool IList.IsReadOnly => true;

    bool IList.IsFixedSize => true;

    int IList.Add(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    bool IList.Contains(object? value) => value is T item && _items.Contains(item);

    int IList.IndexOf(object? value) => value is T item ? _items.IndexOf(item) : -1;

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    object? IList.this[int index]
    {
        get => _items[index];
        set => throw new NotSupportedException();
    }

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection) _items).SyncRoot;

    void ICollection.CopyTo(Array array, int index) => ((ICollection) _items).CopyTo(array, index);

    #endregion
}

file static class CompositeObservableCollectionEventArgs
{
    public static readonly PropertyChangedEventArgs CountChanged = new(nameof(IReadOnlyCollection<>.Count));

    public static readonly PropertyChangedEventArgs IndexerChanged = new("Item[]");

    public static readonly NotifyCollectionChangedEventArgs ResetChanged = new(NotifyCollectionChangedAction.Reset);
}
