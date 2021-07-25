using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mako.Engine;
using Mako.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class IllustrationGridViewModel : ObservableObject
    {
        public IFetchEngine<Illustration?>? FetchEngine { get; set; }

        public ConditionalObservableCollection<IllustrationViewModel> Illustrations { get; }

        public ConditionalObservableCollection<IllustrationViewModel> SelectedIllustrations { get; }


        private bool _isAnyIllustrationSelected;

        public bool IsAnyIllustrationSelected
        {
            get => _isAnyIllustrationSelected;
            set => SetProperty(ref _isAnyIllustrationSelected, value);
        }

        public IllustrationGridViewModel()
        {
            SelectedIllustrations = new ConditionalObservableCollection<IllustrationViewModel>();
            Illustrations = new ConditionalObservableCollection<IllustrationViewModel>();
        }

        public async Task Fill(Action<ObservableCollection<IllustrationViewModel>, IllustrationViewModel> insertAction)
        {
            var added = new HashSet<long>(); 
            await foreach (var illustration in FetchEngine!)
            {
                if (illustration is not null && !added.Contains(illustration.Id) /* Check for the repetition */)
                {
                    if (added.Count >= 500)
                    {
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
                    insertAction(Illustrations, viewModel);
                }
            }
        }

        public async Task Fill(IFetchEngine<Illustration?>? newEngine)
        {
            FetchEngine = newEngine;
            await Fill((models, model) => models.Add(model));
        }

        public async Task ResetAndFill(IFetchEngine<Illustration?>? newEngine, Action<ObservableCollection<IllustrationViewModel>, IllustrationViewModel>? insertAction = null)
        {
            FetchEngine?.EngineHandle.Cancel();
            FetchEngine = newEngine;
            lock (Illustrations)
            {
                Illustrations.Clear();
            }

            lock (SelectedIllustrations)
            {
                SelectedIllustrations.Clear();
            }

            await Fill(insertAction ?? ((models, model) => models.Add(model)));
        }
    }
}