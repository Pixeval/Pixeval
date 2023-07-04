#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/JustifiedLayoutIllustrationViewDataProvider.cs
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

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.Utilities;

namespace Pixeval.UserControls.IllustrationView;

public class IllustrationViewDataProvider : ObservableObject, IIllustrationViewDataProvider, IDisposable
{
    public IllustrationViewDataProvider()
    {
        IllustrationsView = new AdvancedCollectionView(IllustrationsSource);
        IllustrationsSource.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
    }

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public AdvancedCollectionView IllustrationsView { get; }

    private ObservableCollection<IllustrationViewModel> _illustrationSource = new();

    public ObservableCollection<IllustrationViewModel> IllustrationsSource
    {
        get => _illustrationSource;
        protected set
        {
            SetProperty(ref _illustrationSource, value);
            IllustrationsView.Source = value;
        }
    }

    private Predicate<object>? _filter;

    public Predicate<object>? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            _filterChanged?.Invoke(_filter, EventArgs.Empty);
        }
    }

    private EventHandler? _filterChanged;

    public event EventHandler? FilterChanged
    {
        add => _filterChanged += value;
        remove => _filterChanged -= value;
    }

    public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; set; } = new();

    public void DisposeCurrent()
    {
        foreach (var illustrationViewModel in IllustrationsSource)
        {
            illustrationViewModel.Dispose();
        }

        SelectedIllustrations.Clear();
        IllustrationsView.Clear();
        SelectedIllustrations.Clear();
    }

    public virtual Task<int> LoadMore()
    {
        //TODO: delete
        return Task.FromResult(0);
    }

    public virtual async Task<int> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        IllustrationsSource = collection;
        IllustrationsSource.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
        var result = await collection.LoadMoreItemsAsync(20);
        return (int)result.Count;
    }

    public Task<int> ResetAndFillAsync(IFetchEngine<Illustration?>? fetchEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        DisposeCurrent();
        return FillAsync(itemLimit);
    }

    protected virtual void OnIllustrationsSourceOnCollectionChanged(object? _, NotifyCollectionChangedEventArgs args)
    {
        void OnIsSelectionChanged(object? sender, IllustrationViewModel model)
        {
            // Do not add to collection is the model does not conform to the filter
            if (!Filter?.Invoke(model) ?? false)
            {
                return;
            }
            if (model.IsSelected)
            {
                SelectedIllustrations.Add(model);
            }
            else
            {
                SelectedIllustrations.Remove(model);
            }
        }

        switch (args)
        {
            case { Action: NotifyCollectionChangedAction.Add }:
                args.NewItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged += OnIsSelectionChanged);
                break;
            case { Action: NotifyCollectionChangedAction.Remove }:
                args.NewItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged -= OnIsSelectionChanged);
                break;
        }
    }

    public void Dispose()
    {
        DisposeCurrent();
        FetchEngine = null;
    }
}
