// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Pixeval.Controls;

namespace Pixeval.Utilities;

/// <summary>
/// The <see cref="ObservableCollectionAdapter{TInput,TOutput}"/> is a one-way synchronizer between a source observable list of type
/// <typeparamref name="TInput"/> and a destination observable list of <typeparamref name="TOutput"/>.
/// 
/// This class is intended to serve as a one-way map between a collection of models and a collection of
/// ViewModels though can be used for arbitrary purpose. It is recommended the destination element should
/// contain a reference to the source element and propagate changes to the destination element via
/// <see cref="INotifyPropertyChanged"/> or direct mapping to the source. 
/// </summary>
/// <typeparam name="TInput">Source element type (e.g. Model class)</typeparam>
/// <typeparam name="TOutput">Destination element type (e.g. ViewModel class)</typeparam>
public class ObservableCollectionAdapter<TInput, TOutput> : ObservableCollection<TOutput>
    where TInput : class where TOutput : class, IFactory<TInput, TOutput>
{
    private ObservableCollection<TInput> _sourceCollection;

    /// <summary>
    /// Initializes a ObservableCollectionAdapter with a collection implementing IObservable collection.
    /// </summary>
    /// <param name="sourceCollection"></param>
    /// <param name="filter"></param>
    public ObservableCollectionAdapter(ObservableCollection<TInput> sourceCollection, Func<TInput, bool>? filter = null)
    {
        _filter = filter;
        SourceCollection = sourceCollection;
    }

    private readonly Func<TInput, bool>? _filter;

    /// <summary>
    /// A collection that implements IObservableCollection that the Adapter can watch.
    /// </summary>
    public ObservableCollection<TInput> SourceCollection
    {
        get => _sourceCollection;
        [MemberNotNull(nameof(_sourceCollection))]
        init
        {
            if (_sourceCollection is not null)
                _sourceCollection.CollectionChanged -= SourceCollectionChanged;
            _sourceCollection = value;

            SourceCollection.CollectionChanged += SourceCollectionChanged;

            Clear();
            for (var index = 0; index < _sourceCollection.Count; index++)
            {
                var element = _sourceCollection[index];
                var item = TOutput.CreateInstance(element);
                Add(item);
            }
        }
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.NewItems is null)
                    return;
                for (var i = 0; i < args.NewItems.Count; ++i)
                {
                    var input = (TInput) args.NewItems[i]!;
                    if (_filter?.Invoke(input) is false)
                        continue;
                    var item = TOutput.CreateInstance(input);
                    Insert(args.NewStartingIndex + i, item);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is null)
                    return;
                for (var i = 0; i < args.OldItems.Count; ++i)
                    RemoveAt(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is null)
                    return;
                var removedCount = 0;
                for (var i = 0; i < args.NewItems.Count; ++i)
                {
                    var input = (TInput) args.NewItems[i]!;
                    if (_filter?.Invoke(input) is false)
                    {
                        RemoveAt(args.OldStartingIndex + i - removedCount);
                        ++removedCount;
                    }
                    else
                    {
                        var item = TOutput.CreateInstance(input);
                        this[args.OldStartingIndex + i - removedCount] = item;
                    }
                }
                break;
            case NotifyCollectionChangedAction.Move:
                if (args.OldItems is null)
                    return;
                for (var i = 0; i < args.OldItems.Count; ++i)
                    Move(args.OldStartingIndex + i, args.NewStartingIndex + i);
                break;
            case NotifyCollectionChangedAction.Reset:
                Clear();
                for (var index = 0; index < SourceCollection.Count; index++)
                {
                    var element = SourceCollection[index];
                    if (_filter?.Invoke(element) is false)
                        continue;
                    var item = TOutput.CreateInstance(element);
                    Add(item);
                }

                break;
            default:
                ThrowUtils.ArgumentOutOfRange(args.Action);
                return;
        }
    }
}
