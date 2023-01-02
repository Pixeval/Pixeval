#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/GridIllustrationViewDataProvider.cs
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
using Pixeval.UserControls;
using Pixeval.Utilities;

namespace Pixeval;

public partial class GridIllustrationViewDataProvider : ObservableObject, IIllustrationViewDataProvider, IDisposable
{
    public GridIllustrationViewDataProvider()
    {
        _illustrationSource = new ObservableCollection<IllustrationViewModel>();
        SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
        IllustrationsView = new AdvancedCollectionView(IllustrationsSource);
        IllustrationsSource.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
    }

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public AdvancedCollectionView IllustrationsView { get; }

    private ObservableCollection<IllustrationViewModel> _illustrationSource;

    public ObservableCollection<IllustrationViewModel> IllustrationsSource
    {
        get => _illustrationSource;
        private set
        {
            SetProperty(ref _illustrationSource, value);
            IllustrationsView.Source = value;
        }
    }

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

    public bool Sortable => true;

    public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; set; }

    public async Task<bool> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        IllustrationsSource = collection;
        await collection.LoadMoreItemsAsync(20);
        IllustrationsSource.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
        return IllustrationsSource.Count > 0;
    }

    public Task FillAsync(IFetchEngine<Illustration?>? fetchEngine, int? itemLimit = null)
    {
        FetchEngine = fetchEngine;
        return FillAsync(itemLimit);
    }

    public Task<bool> ResetAndFillAsync(IFetchEngine<Illustration?>? fetchEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        DisposeCurrent();
        return FillAsync(itemLimit);
    }

    private void OnIllustrationsSourceOnCollectionChanged(object? _, NotifyCollectionChangedEventArgs args)
    {
        void OnIsSelectionChanged(object? sender, IllustrationViewModel model)
        {
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