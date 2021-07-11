using Microsoft.UI.Xaml;
using PropertyChanged;

namespace Pixeval.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel
    {
        public double MainPageRootNavigationViewOpenPanelLength => 200;

        public double MainPageAutoSuggestionBoxWidth => 300;

        public Thickness RearrangeMainPageAutoSuggestionBoxMargin(double windowWidth, double leftControlWidth)
        {
            return new(windowWidth / 2 - leftControlWidth - MainPageAutoSuggestionBoxWidth / 2, 0, 0, 0);
        }
    }
}