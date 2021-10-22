#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationGridViewModel.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.CommunityToolkit.AdvancedCollectionView;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.ViewModel
{
    public class IllustrationGridViewModel : ObservableObject, IDisposable
    {
        private bool _isAnyIllustrationSelected;

        private string _selectionLabel;

        public IllustrationGridViewModel()
        {
            SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
            Illustrations = new ObservableCollection<IllustrationViewModel>();
            IllustrationsView = new AdvancedCollectionView(Illustrations);
            _selectionLabel = IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel;
        }

        public IFetchEngine<Illustration?>? FetchEngine { get; set; }

        public ObservableCollection<IllustrationViewModel> Illustrations { get; }

        public AdvancedCollectionView IllustrationsView { get; }

        public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; }

        public bool IsAnyIllustrationSelected
        {
            get => _isAnyIllustrationSelected;
            set => SetProperty(ref _isAnyIllustrationSelected, value);
        }

        public string SelectionLabel
        {
            get => _selectionLabel;
            set => SetProperty(ref _selectionLabel, value);
        }

        public void Dispose()
        {
            DisposeCurrent();
            GC.SuppressFinalize(this);
        }

        public async Task FillAsync(int? itemsLimit = null)
        {
            var added = new HashSet<long>();
            await foreach (var illustration in FetchEngine!)
            {
                if (illustration is not null && !added.Contains(illustration.Id) /* Check for the repetition */)
                {
                    if (added.Count >= itemsLimit)
                    {
                        FetchEngine.Cancel();
                        break;
                    }

                    added.Add(illustration.Id); // add to the already-added-illustration list
                    var viewModel = new IllustrationViewModel(illustration);
                    viewModel.OnIsSelectedChanged += (_, model) => // add/remove the viewModel to/from SelectedIllustrations according to the IsSelected Property
                    {
                        if (model.IsSelected)
                        {
                            SelectedIllustrations.Add(model);
                        }
                        else
                        {
                            SelectedIllustrations.Remove(model);
                        }

                        // Update the IsAnyIllustrationSelected Property if any of the viewModel's IsSelected property changes
                        IsAnyIllustrationSelected = SelectedIllustrations.Any();

                        var count = SelectedIllustrations.Count;
                        SelectionLabel = count == 0
                            ? IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel
                            : IllustrationGridCommandBarResources.CancelSelectionButtonFormatted.Format(count);
                    };
                    IllustrationsView.Add(viewModel);
                }
            }
        }

        public async Task FillAsync(IFetchEngine<Illustration?>? newEngine, int? itemsLimit = null)
        {
            FetchEngine = newEngine;
            await FillAsync(itemsLimit);
        }

        public async Task ResetAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
        {
            FetchEngine?.EngineHandle.Cancel();
            FetchEngine = newEngine;
            DisposeCurrent();
            await FillAsync(itemLimit);
        }

        public void SetSortDescription(SortDescription description)
        {
            if (!IllustrationsView.SortDescriptions.Any())
            {
                IllustrationsView.SortDescriptions.Add(description);
                return;
            }

            IllustrationsView.SortDescriptions[0] = description;
        }

        public void ClearSortDescription()
        {
            IllustrationsView.SortDescriptions.Clear();
        }

        private void DisposeCurrent()
        {
            foreach (IllustrationViewModel illustrationViewModel in IllustrationsView)
            {
                illustrationViewModel.Dispose();
            }

            SelectedIllustrations.Clear();
            IllustrationsView.Clear();
        }

        ~IllustrationGridViewModel()
        {
            Dispose();
        }
    }
}