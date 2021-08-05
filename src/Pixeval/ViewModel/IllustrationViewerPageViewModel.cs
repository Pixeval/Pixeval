using Microsoft.Toolkit.Mvvm.ComponentModel;

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
            Illustrations = illustrations;
            Current = Illustrations[CurrentIndex];
        }

        public IllustrationViewModel[] Illustrations { get; }

        private int _currentIndex;

        public int CurrentIndex
        {
            get => _currentIndex;
            private set => SetProperty(ref _currentIndex, value);
        }

        private IllustrationViewModel _current = null!;

        public IllustrationViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        public bool IsManga => Illustrations.Length > 1;

        public IllustrationViewModel Next()
        {
            Current = Illustrations[CurrentIndex + 1 >= Illustrations.Length ? CurrentIndex : ++CurrentIndex];
            return Current;
        }

        public IllustrationViewModel Prev()
        {
            Current = Illustrations[CurrentIndex - 1 < 0 ? CurrentIndex : --CurrentIndex];
            return Current;
        }
    }
}