using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.Pages.Capability
{
    public sealed partial class SearchResultsPage : ISortedIllustrationContainerPageHelper
    {
        public IllustrationContainer ViewModelProvider => IllustrationContainer;

        public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

        public SearchResultsPage()
        {
            InitializeComponent();
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationContainer.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
        {
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            ChangeSource((IFetchEngine<Illustration>) navigationEventArgs.Parameter);
            WeakReferenceMessenger.Default.Register<SearchResultsPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private void ChangeSource(IFetchEngine<Illustration> engine)
        {
            _ = IllustrationContainer.ViewModel.ResetAndFillAsync(engine);
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
        }
    }
}
