using Mako.Model;
using Microsoft.UI.Xaml.Media;
using PropertyChanged;

namespace Pixeval.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class IllustrationThumbnailViewModel
    {
        [DoNotNotify]
        public Illustration Illustration { get; }

        public IllustrationThumbnailViewModel(Illustration illustration)
        {
            Illustration = illustration;
            IsBookmarked = illustration.IsBookmarked;
            Id = illustration.Id.ToString();
            IsSelected = false;
            ThumbnailSource = null!;
        }

        public string Id { get; set; }

        public bool IsSelected { get; set; }

        public bool IsBookmarked { get; set; }

        public ImageSource ThumbnailSource { get; private set; }
    }
}