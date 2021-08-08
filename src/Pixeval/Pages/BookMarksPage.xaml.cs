using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;

namespace Pixeval.Pages
{
    public sealed partial class BookmarksPage
    {
        public BookmarksPage()
        {
            InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxPublicItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        }

        private async void BookmarksPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ChangeSource();
        }

        private async void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private async void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private async Task ChangeSource()
        {
            if (TryGetPrivacyPolicy(PrivacyPolicyComboBox, out var policy))
            {
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Bookmarks(App.Uid!, policy), MakoHelper.Insert(GetIllustrationSortOption()));
            }
        }

        #region Helper Functions

        private static bool TryGetPrivacyPolicy(ComboBox sender, out PrivacyPolicy type)
        {
            if (sender is { SelectedItem: ComboBoxItem { Tag: PrivacyPolicy t } })
            {
                type = t;
                return true;
            }

            type = PrivacyPolicy.Public;
            return false;
        }

        private IllustrationSortOption GetIllustrationSortOption()
        {
            return ((IllustrationSortOptionWrapper) SortOptionComboBox.SelectedItem).Value;
        }

        #endregion
    }
}