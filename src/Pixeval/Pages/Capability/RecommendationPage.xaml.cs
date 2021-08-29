using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Mako.Global.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
            IllustrationGrid.ViewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            EventChannel.Default.Subscribe<MainPageFrameNavigatingEvent>(() => IllustrationGrid.ViewModel.FetchEngine?.Cancel());
        }

        private async void RecommendationsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                await ChangeSource();
            }

            IllustrationGrid.Focus(FocusState.Programmatic);
        }

        private async void ModeSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = IllustrationGrid.ViewModel;
            if (MakoHelper.GetSortDescriptionForIllustration(GetIllustrationSortOption()) is { } desc)
            {
                viewModel.SetSortDescription(desc);
                if (viewModel.IllustrationsView.FirstOrDefault() is { } first)
                {
                    IllustrationGrid.FindChild<GridView>()?.ScrollIntoView(first);
                }
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
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Recommends(type), App.AppSetting.ItemsNumberLimitForDailyRecommendations);
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
