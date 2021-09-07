using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Events;
using Pixeval.Options;
using Pixeval.Util;

namespace Pixeval.Pages.Capability
{
    public sealed partial class RecommendationPage
    {
        public RecommendationPage()
        {
            InitializeComponent();
        }

        public override void Dispose(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationContainer.ViewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            EventChannel.Default.Subscribe<MainPageFrameNavigatingEvent>(() => IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private async void RecommendationsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                await ChangeSource();
            }
        }

        private async void ModeSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = IllustrationContainer.ViewModel;
            if (MakoHelper.GetSortDescriptionForIllustration(GetIllustrationSortOption()) is { } desc)
            {
                viewModel.SetSortDescription(desc);
                IllustrationContainer.ScrollToTop();
            }
            else
            {
                viewModel.ClearSortDescription();
            }
        }

        private async Task ChangeSource()
        {
            if (TryGetRecommendContentType(ModeSelectionComboBox, out var type))
            {
                await IllustrationContainer.ViewModel.ResetAndFill(App.MakoClient.Recommends(type), App.AppSetting.ItemsNumberLimitForDailyRecommendations);
            }
        }

        #region Helper Functions

        private static bool TryGetRecommendContentType(ComboBox sender, out RecommendContentType type)
        {
            if (sender is {SelectedItem: ComboBoxItem {Tag: RecommendContentType t}})
            {
                type = t;
                return true;
            }

            type = RecommendContentType.Illust;
            return false;
        }

        private IllustrationSortOption GetIllustrationSortOption()
        {
            return ((IllustrationSortOptionWrapper) SortOptionComboBox.SelectedItem).Value;
        }

        #endregion
    }
}
