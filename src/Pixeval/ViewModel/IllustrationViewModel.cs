using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModel
{
    public class IllustrationViewModel : ObservableObject
    {
        private readonly Illustration _illustration;

        public string Id => _illustration.Id.ToString();

        public string Title => _illustration.Title ?? string.Empty;

        public bool IsBookmarked
        {
            get => _illustration.IsBookmarked;
            set => SetProperty(_illustration.IsBookmarked, value, m => _illustration.IsBookmarked = m);
        }

        public bool Selected { get; set; }

        public IllustrationViewModel(Illustration illustration)
        {
            _illustration = illustration;
        }

        public Task PostPublicBookmarkAsync()
        {
            return App.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
        }
    }
}