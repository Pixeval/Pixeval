using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mako.Engine;
using Mako.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModel
{
    public class IllustrationGridPageViewModel : ObservableObject
    {
        public IFetchEngine<Illustration?>? FetchEngine { get; set; }

        public ObservableCollection<IllustrationViewModel> Illustrations { get; }

        public IllustrationGridPageViewModel()
        {
            Illustrations = new ObservableCollection<IllustrationViewModel>();
        }

        public async Task Fill()
        {
            // TODO: The cache system should be take into our considerations
            // The cache system MIGHT improve the fluency of the UI, but it may
            // also not worth the efforts
            await foreach (var illustration in FetchEngine!)
            {
                // TODO: Use a global static object (by using IoC or a static property)
                // to manage all the filter options, such as minimum bookmark, maximum
                // images that are allowed to be loaded at the same time, required tags
                // and excluded tags, etc.
                //
                // if (FetchEngine.RequestedPages >= 20)
                // {
                //     FetchEngine.EngineHandle.Cancel();
                // }
                if (illustration is not null)
                {
                    Illustrations.Add(new IllustrationViewModel(illustration));
                }
            }
        }

        public void Reset()
        {
            FetchEngine?.EngineHandle.Cancel();
            Illustrations.Clear();
        }

        public async Task ResetAndFill(IFetchEngine<Illustration?>? newEngine)
        {
            Reset();
            FetchEngine = newEngine;
            await Fill();
        }
    }
}