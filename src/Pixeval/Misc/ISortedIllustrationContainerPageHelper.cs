using Pixeval.UserControls;
using Pixeval.Util.Generic;

namespace Pixeval.Misc
{
    public interface ISortedIllustrationContainerPageHelper
    {
        IllustrationContainer ViewModelProvider { get; }

        SortOptionComboBox SortOptionProvider { get; }

        public void OnSortOptionChanged()
        {
            switch (SortOptionProvider.GetSortDescription())
            {
                case { } desc:
                    ViewModelProvider.ViewModel.SetSortDescription(desc);
                    ViewModelProvider.ScrollToTop();
                    break;
                default:
                    // reset the view so that it can resort its item to the initial order
                    ViewModelProvider.ViewModel.ClearSortDescription();
                    ViewModelProvider.ViewModel.IllustrationsView.ResetView();
                    ViewModelProvider.ScrollToTop();
                    break;
            }
        }
    }
}