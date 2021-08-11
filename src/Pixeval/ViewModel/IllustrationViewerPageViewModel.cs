using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class IllustrationViewerPageViewModel : ObservableObject
    {
        /// <summary>
        /// <see cref="illustrations"/> should contains only one item iff the illustration is a single
        /// otherwise it contains the entire manga data
        /// </summary>
        public IllustrationViewerPageViewModel(params IllustrationViewModel[] illustrations)
        {
            Illustrations = illustrations.Select(i => new ImageViewerPageViewModel(i)).ToArray();
            Current = Illustrations[CurrentIndex];
        }

        public ImageViewerPageViewModel[] Illustrations { get; }

        private int _currentIndex;

        public int CurrentIndex
        {
            get => _currentIndex;
            private set => SetProperty(ref _currentIndex, value);
        }

        private ImageViewerPageViewModel _current = null!;

        public ImageViewerPageViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        public bool IsManga => Illustrations.Length > 1;

        public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

        public IllustrationViewModel First => Illustrations[0].IllustrationViewModel;

        public Visibility CalculateNextImageButtonVisibility(int index)
        {
            return index < Illustrations.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CalculatePrevImageButtonVisibility(int index)
        {
            return index > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public ImageViewerPageViewModel Next()
        {
            Current = Illustrations[++CurrentIndex];
            return Current;
        }

        public ImageViewerPageViewModel Prev()
        {
            Current = Illustrations[--CurrentIndex];
            return Current;
        }

        public Task PostPublicBookmarkAsync()
        {
            return Illustrations[0].IllustrationViewModel.PostPublicBookmarkAsync();
        }

        public Task RemoveBookmarkAsync()
        {
            return Illustrations[0].IllustrationViewModel.RemoveBookmarkAsync();
        }

        public bool IsBookmarked => Illustrations[0].IllustrationViewModel.IsBookmarked;
    }
}