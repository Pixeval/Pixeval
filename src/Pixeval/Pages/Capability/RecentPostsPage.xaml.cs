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
    public sealed partial class RecentPostsPage : ISortedIllustrationContainerPageHelper
    {
        public IllustrationContainer ViewModelProvider => IllustrationContainer;

        public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

        public RecentPostsPage()
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
            PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxPublicItem;
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            WeakReferenceMessenger.Default.Register<RecentPostsPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private void RecentPostsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.AppViewModel.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                ChangeSource();
            }
        }

        private void PrivacyPolicyComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
        {
            ChangeSource();
        }

        private void ChangeSource()
        {
            _ = IllustrationContainer.ViewModel.ResetAndFillAsync(App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicyComboBox.GetComboBoxSelectedItemTag(PrivacyPolicy.Public)));
        }

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
        }
    }
}
