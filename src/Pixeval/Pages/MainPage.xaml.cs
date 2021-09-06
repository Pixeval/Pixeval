using System;
using System.Runtime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Events;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Misc;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class MainPage
    {
        private readonly MainPageViewModel _viewModel = new();

        private readonly NavigationViewTag _recommends = new(typeof(RecommendationPage), App.MakoClient.Recommends(targetFilter: App.AppSetting.TargetFilter));

        private readonly NavigationViewTag _bookmarks = new(typeof(BookmarksPage), App.MakoClient.Bookmarks(App.Uid!, PrivacyPolicy.Public, App.AppSetting.TargetFilter));

        private readonly NavigationViewTag _rankings = new(typeof(RankingsPage), App.MakoClient.Ranking(RankOption.Day, DateTime.Today - TimeSpan.FromDays(2)));

        private readonly NavigationViewTag _recentPosts = new(typeof(RecentPostsPage), App.MakoClient.RecentPosts(PrivacyPolicy.Public));

        private readonly NavigationViewTag _settings = new(typeof(SettingsPage), App.MakoClient.Configuration);

        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
         
        private void MainPageRootNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainPageRootFrame.NavigateByNavigationViewTag(sender, new EntranceNavigationTransitionInfo());
        }

        private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            EventChannel.Default.Publish(new MainPageFrameNavigatingEvent(this));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        // 这玩意放在这有点蠢
        public static UIElement? selectedElement = null;

        // 转移到MainPage中某element的动画效果
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim == null) return;
            anim.Configuration = new DirectConnectedAnimationConfiguration();
            anim.TryStart(selectedElement != null ? selectedElement! : this);
        }
    }
}
