#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2024 Pixeval.Utilities/ObservableCollectionAdapter.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using Pixeval.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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
    public ObservableCollectionAdapter(ObservableCollection<TInput> sourceCollection) => SourceCollection = sourceCollection;

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
            foreach (var element in _sourceCollection)
            {
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
                    var item = TOutput.CreateInstance((TInput)args.NewItems[i]!);
                    Insert(args.NewStartingIndex + i, item!);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is null)
                    return;
                for (var i = 0; i < args.OldItems.Count; ++i)
                    RemoveAt(args.OldStartingIndex + i);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is null)
                    return;
                for (var i = 0; i < args.NewItems.Count; ++i)
                {
                    var item = TOutput.CreateInstance((TInput)args.NewItems[i]!);
                    this[args.OldStartingIndex + i] = item;
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
                foreach (var element in SourceCollection)
                {
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
