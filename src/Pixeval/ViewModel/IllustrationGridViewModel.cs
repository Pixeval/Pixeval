using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Mako.Engine;
using Mako.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class IllustrationGridViewModel : ObservableObject
    {
        private static readonly object Lock = new();

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

        public IllustrationGridViewModel()
        {
            SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
            Illustrations = new ObservableCollection<IllustrationViewModel>();
            IllustrationsView = new AdvancedCollectionView(Illustrations);
        }

        public async Task Fill(int? itemsLimit = null)
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
                    };
                    IllustrationsView.Add(viewModel);
                }
            }
        }

        public async Task Fill(IFetchEngine<Illustration?>? newEngine, int? itemsLimit = null)
        {
            FetchEngine = newEngine;
            await Fill(itemsLimit);
        }

        public async Task ResetAndFill(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
        {
            FetchEngine?.EngineHandle.Cancel();
            FetchEngine = newEngine;
            lock (Lock)
            {
                IllustrationsView.Clear();
                SelectedIllustrations.Clear();
            }
            await Fill(itemLimit);
        }

        public void Dispose()
        {
            SelectedIllustrations.Clear();
            IllustrationsView.Clear();
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

        public void ClearSortDescription(SortDescription description)
        {
            IllustrationsView.SortDescriptions.Clear();
        }
    }
}