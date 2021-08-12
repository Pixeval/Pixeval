using System;
using System.Runtime;
using Mako.Global.Enum;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Events;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    // This class cannot be put inside the MainPage due to the property of x:Bind
    public sealed class MainPageRootNavigationViewTag
    {
        public Type NavigateTo { get; }

        public object Parameter { get; }

        public MainPageRootNavigationViewTag(Type navigateTo, object parameter)
        {
            NavigateTo = navigateTo;
            Parameter = parameter;
        }

        public static readonly MainPageRootNavigationViewTag Recommends = new(typeof(RecommendationPage), App.MakoClient.Recommends(targetFilter: App.AppSetting.TargetFilter));
        
        public static readonly MainPageRootNavigationViewTag Bookmarks = new(typeof(BookmarksPage), App.MakoClient.Bookmarks(App.Uid!, PrivacyPolicy.Public, App.AppSetting.TargetFilter));

        public static readonly MainPageRootNavigationViewTag Settings = new(typeof(SettingsPage), App.MakoClient.Configuration);
    }

    public sealed partial class MainPage
    {
        private readonly MainPageViewModel _viewModel = new();

        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void MainPageRootNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem {Tag: MainPageRootNavigationViewTag tag})
            {
                MainPageRootFrame.Navigate(tag.NavigateTo, tag.Parameter, new DrillInNavigationTransitionInfo());
            }
        }

        private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            EventChannel.Default.Publish(new MainPageFrameNavigatingEvent(this));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        } 
    }
}
