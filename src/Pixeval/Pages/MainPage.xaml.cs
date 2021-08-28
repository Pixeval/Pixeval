using System;
using System.Runtime;
using Mako.Global.Enum;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
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

        private readonly NavigationViewTag _rankings = new(typeof(RankingsPage), App.MakoClient.Ranking(RankOption.Day, DateTime.Now));
        
        private readonly NavigationViewTag _settings = new(typeof(SettingsPage), App.MakoClient.Configuration);

        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void MainPageRootNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainPageRootFrame.NavigateByNavigationViewTag(sender, new DrillInNavigationTransitionInfo());
        }

        private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            EventChannel.Default.Publish(new MainPageFrameNavigatingEvent(this));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        } 
    }
}
