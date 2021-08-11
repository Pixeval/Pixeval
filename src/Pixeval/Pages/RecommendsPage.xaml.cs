using System.Threading.Tasks;
using Mako.Global.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;

namespace Pixeval.Pages
{
    public sealed partial class RecommendsPage
    {
        public RecommendsPage()
        {
            InitializeComponent();
        }

        public override void Dispose()
        {
            IllustrationGrid.ViewModel.Dispose();
        }

        public override void Prepare()
        {
            ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        }

        private async void RecommendsPage_OnLoaded(object sender, RoutedEventArgs e)
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

        private async void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private async Task ChangeSource()
        {
            if (TryGetRecommendContentType(ModeSelectionComboBox, out var type))
            {
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Recommends(type), MakoHelper.Insert(GetIllustrationSortOption()));
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
