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
        public IFetchEngine<Illustration?>? FetchEngine { get; set; }

        public ObservableCollection<IllustrationViewModel> Illustrations { get; }

        public AdvancedCollectionView IllustrationsView { get; }

        public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; }

        private bool _isAnyIllustrationSelected;

        public bool IsAnyIllustrationSelected
        {
            get => _isAnyIllustrationSelected;
            set => SetProperty(ref _isAnyIllustrationSelected, value);
        }

        private string _selectionLabel;

        public string SelectionLabel
        {
            get => _selectionLabel;
            set => SetProperty(ref _selectionLabel, value);
        }

        public IllustrationGridViewModel()
        {
            SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
            Illustrations = new ObservableCollection<IllustrationViewModel>();
            IllustrationsView = new AdvancedCollectionView(Illustrations);
            _selectionLabel = IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel;
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

        public void Dispose()
        {
            DisposeCurrent();
            GC.SuppressFinalize(this);
        }

        ~IllustrationGridViewModel()
        {
            Dispose();
        }
    }
}