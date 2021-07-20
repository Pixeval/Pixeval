using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mako.Engine;
using Mako.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModel
{
    public class IllustrationGridPageViewModel : ObservableObject
    {
        public IFetchEngine<Illustration>? FetchEngine { get; set; }

        public ObservableCollection<IllustrationViewModel> Illustrations { get; }

        public IllustrationGridPageViewModel()
        {
            Illustrations = new ObservableCollection<IllustrationViewModel>();
        }

        public async Task Fill()
        {
            await foreach (var illustration in FetchEngine!)
            {
                Illustrations.Add(new IllustrationViewModel(illustration));
            }
        }
    }
}