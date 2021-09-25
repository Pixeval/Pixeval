using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Capability
{
    public sealed partial class RecommendationPage : ISortedIllustrationContainerPageHelper
    {
        public IllustrationContainer ViewModelProvider => IllustrationContainer;

        public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

        public RecommendationPage()
        {
            InitializeComponent();
        }

        public override void Dispose(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationContainer.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            WeakReferenceMessenger.Default.Register<RecommendationPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private void RecommendationsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.AppViewModel.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            { 
                ChangeSource();
            }
        }

        private void ModeSelectionComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
        { 
            ChangeSource();
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // FUCK C#, the default implementations are not inherited. We have to use this stupid cast here.
            // even a donkey knows "this" is an "ISortedIllustrationContainerPageHelper"
            ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
        }

        private void ChangeSource()
        {
            _ = IllustrationContainer.ViewModel.ResetAndFillAsync(App.AppViewModel.MakoClient.Recommends(ModeSelectionComboBox.GetComboBoxSelectedItemTag(RecommendContentType.Illust)), App.AppViewModel.AppSetting.ItemsNumberLimitForDailyRecommendations);
        }
    }
}
