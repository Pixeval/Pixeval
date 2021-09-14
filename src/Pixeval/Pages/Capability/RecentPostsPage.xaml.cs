using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval.Pages.Capability
{
    public sealed partial class RecentPostsPage
    {
        public RecentPostsPage()
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

        private async void RecentPostsPage_OnLoaded(object sender, RoutedEventArgs e)
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

        private async Task ChangeSource()
        {
            if (TryGetPrivacyPolicy(PrivacyPolicyComboBox, out var policy))
            {
                await IllustrationContainer.ViewModel.ResetAndFill(App.MakoClient.RecentPosts(policy));
            }
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = IllustrationContainer.ViewModel;
            if (MakoHelper.GetSortDescriptionForIllustration(SortOptionComboBox.SelectedOption) is { } desc)
            {
                viewModel.SetSortDescription(desc);
                IllustrationContainer.ScrollToTop();
            }
            else
            {
                viewModel.ClearSortDescription();
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

        #endregion
    }
}
