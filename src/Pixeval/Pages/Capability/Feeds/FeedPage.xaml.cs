using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Documents;
using Pixeval.CoreApi.Model;
using WinUI3Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability.Feeds
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedPage
    {
        private FeedItemViewModel? _lastSelected;

        public FeedPage()
        {
            InitializeComponent();
            _viewModel = new FeedPageViewModel();
        }

        private readonly FeedPageViewModel _viewModel;

        private void FeedPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DataProvider.ResetEngine(App.AppViewModel.MakoClient.Feeds()!);
        }

        private async void TimelineUnit_OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadFeedItemViewModelAsync((Grid) sender);
        }

        private async void TimelineBlock_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (args.BringIntoViewDistanceY <= 100)
            {
                await LoadFeedItemViewModelAsync((Grid) sender);
            }
        }

        private async Task LoadFeedItemViewModelAsync(FrameworkElement root)
        {
            var vm = root.GetDataContext<FeedItemViewModel>();
            var contentTextBlock = root.FindDescendant("FeedContentTextBlock") as TextBlock;

            contentTextBlock?.Inlines.Clear();
            switch (vm.Entry.Type)
            {
                case FeedType.AddBookmark:
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.BookmarkIllustTemplateSegment1Text });
                    contentTextBlock?.Inlines.Add(GetVmEmphasizedRun(vm.Entry.FeedName ?? string.Empty));
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.BookmarkIllustTemplateSegment2Text });
                    break;
                case FeedType.AddIllust:
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.PostIllustTemplateSegmentText });
                    contentTextBlock?.Inlines.Add(GetVmEmphasizedRun(vm.Entry.FeedName ?? string.Empty));
                    break;
                case FeedType.AddFavorite:
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.FollowUserTemplateSegmentText });
                    contentTextBlock?.Inlines.Add(GetVmEmphasizedRun(vm.Entry.FeedName ?? string.Empty));
                    break;
                case FeedType.AddNovelBookmark:
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.BookmarkNovelTemplateSegment1Text });
                    contentTextBlock?.Inlines.Add(GetVmEmphasizedRun(vm.Entry.FeedName ?? string.Empty));
                    contentTextBlock?.Inlines.Add(new Run { Text = TimelineResources.BookmarkNovelTemplateSegment2Text });
                    break;
            }

            await vm.LoadAsync();
            return;

            Run GetVmEmphasizedRun(string text) => new()
            {
                Text = text,
                FontFamily = new FontFamily("Bahnschrift"),
                Foreground = vm.IconSecondaryBrush,
                FontWeight = new FontWeight(700)
            };
        }

        private void ItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
        {
            _lastSelected?.Select(false);
            var vm = sender.SelectedItem as FeedItemViewModel;
            vm?.Select(true);
            _lastSelected = vm;
        }
   }
}
