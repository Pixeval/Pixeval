using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Events;
using Pixeval.Options;
using Pixeval.Util;

namespace Pixeval.Pages.Capability
{
    public sealed partial class BookmarksPage
    {
        public BookmarksPage()
        {
            InitializeComponent();
        }

        public override void Dispose(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationContainer.ViewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxPublicItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            EventChannel.Default.Subscribe<MainPageFrameNavigatingEvent>(() => IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private async void BookmarksPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                await ChangeSource();
            }
        }

        private async void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = IllustrationContainer.ViewModel;
            if (MakoHelper.GetSortDescriptionForIllustration(GetIllustrationSortOption()) is { } desc)
            {
                viewModel.SetSortDescription(desc);
                if (viewModel.IllustrationsView.FirstOrDefault() is { } first)
                {
                    IllustrationContainer.FindChild<GridView>()?.ScrollIntoView(first);
                }
            }
            else
            {
                viewModel.ClearSortDescription();
            }
        }

        private async Task ChangeSource()
        {
            if (TryGetPrivacyPolicy(PrivacyPolicyComboBox, out var policy))
            {
                await IllustrationContainer.ViewModel.ResetAndFill(App.MakoClient.Bookmarks(App.Uid!, policy));
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