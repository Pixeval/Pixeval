using System;
using System.Diagnostics;
using System.Runtime;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Misc;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class MainPage
    {
        private readonly MainPageViewModel _viewModel = new();

        private readonly NavigationViewTag _recommends = new(typeof(RecommendationPage), App.AppViewModel.MakoClient.Recommends(targetFilter: App.AppViewModel.AppSetting.TargetFilter));

        private readonly NavigationViewTag _bookmarks = new(typeof(BookmarksPage), App.AppViewModel.MakoClient.Bookmarks(App.AppViewModel.PixivUid!, PrivacyPolicy.Public, App.AppViewModel.AppSetting.TargetFilter));

        private readonly NavigationViewTag _rankings = new(typeof(RankingsPage), App.AppViewModel.MakoClient.Ranking(RankOption.Day, DateTime.Today - TimeSpan.FromDays(2)));

        private readonly NavigationViewTag _recentPosts = new(typeof(RecentPostsPage), App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicy.Public));

        private readonly NavigationViewTag _settings = new(typeof(SettingsPage), App.AppViewModel.MakoClient.Configuration);

        private static UIElement? _connectedAnimationTarget;

        // This field contains the view model that the illustration viewer is
        // currently holding if we're navigating back to the MainPage
        private static IllustrationViewModel? _illustrationViewerContent;

        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            WeakReferenceMessenger.Default.Register<MainPage, MainPageFrameSetConnectedAnimationTargetMessage>(this, (_, message) => _connectedAnimationTarget = message.Parameter);
            WeakReferenceMessenger.Default.Register<MainPage, NavigatingBackToMainPageMessage>(this, (_, message) => _illustrationViewerContent = message.Parameter);
        }
         
        private void MainPageRootNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainPageRootFrame.NavigateByNavigationViewTag(sender, new EntranceNavigationTransitionInfo());
        }

        private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new MainPageFrameNavigatingEvent(this));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        // 转移到MainPage中某element的动画效果
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                animation.TryStart(_connectedAnimationTarget ?? this);
                _connectedAnimationTarget = null;
            }

            // Scroll the content to the item that were being browsed just now
            if (_illustrationViewerContent is not null && MainPageRootFrame.FindDescendant<AdaptiveGridView>() is { } gridView)
            {
                gridView.ScrollIntoView(_illustrationViewerContent);
                _illustrationViewerContent = null;
            }
        }

        private void MainPageRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            // TODO
            Debugger.Break();
        }
    }
}
