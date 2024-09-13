using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Documents;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;
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
        private AbstractFeedItemViewModel? _lastSelected;

        public FeedPage()
        {
            InitializeComponent();
            _viewModel = new FeedPageViewModel();
        }

        private readonly FeedPageViewModel _viewModel;

        private void FeedPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DataProvider.ResetEngine(new FeedProxyFetchEngine(App.AppViewModel.MakoClient.Feeds())!);
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
            var contentTextBlock = root.FindDescendant("FeedContentTextBlock") as TextBlock;
            var vm = root.GetDataContext<AbstractFeedItemViewModel>();
            var isSparse = vm is FeedItemSparseViewModel;

            var entry = vm.GetMostSignificantEntry()!;
            var feedNameString = vm is FeedItemCondensedViewModel { Entry: IFeedEntry.CondensedFeedEntry(var entries) } 
                ? GetVmEmphasizedRun(string.Join(", ", entries.Take(2).Select(e => e?.FeedName)))
                : GetVmEmphasizedRun(entry.FeedName ?? string.Empty);
            var entriesLengthIfCondensed = vm is FeedItemCondensedViewModel { Entry: IFeedEntry.CondensedFeedEntry({ Count: var count }) }
                ? count
                : 0;

            contentTextBlock ?.Inlines.Clear();
            switch (entry.Type)
            {
                case FeedType.AddBookmark or FeedType.AddNovelBookmark:
                    contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? FeedPageResources.SparseBookmarkPrefix : FeedPageResources.CondensedBookmarkPrefix });
                    contentTextBlock?.Inlines.Add(feedNameString);
                    contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? FeedPageResources.SparseBookmarkSuffix : FeedPageResources.CondensedBookmarkFormattedSuffix.Format(entriesLengthIfCondensed) });
                    break;
                case FeedType.PostIllust:
                    contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? FeedPageResources.SparsePostIllustPrefix : FeedPageResources.CondensedPostIllustPrefix });
                    contentTextBlock?.Inlines.Add(feedNameString);
                    if (!isSparse)
                        contentTextBlock?.Inlines.Add(new Run { Text = FeedPageResources.CondensedPostIllustFormattedSuffix.Format(entriesLengthIfCondensed) });
                    break;
                case FeedType.AddFavorite:
                    contentTextBlock?.Inlines.Add(new Run { Text = FeedPageResources.SparseFollowUserPrefix });
                    contentTextBlock?.Inlines.Add(feedNameString);
                    if (!isSparse)
                        contentTextBlock?.Inlines.Add(new Run { Text = FeedPageResources.CondensedFollowUserFormattedSuffix.Format(entriesLengthIfCondensed) });
                    break;
            }

            await vm.LoadAsync();

            return;

            Run GetVmEmphasizedRun(string text) => new()
            {
                Text = text,
                FontFamily = new FontFamily("Bahnschrift"),
                Foreground = vm.FeedBrush,
                FontWeight = new FontWeight(700)
            };
        }

        private void ItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
        {
            _lastSelected?.Select(false);
            var vm = sender.SelectedItem as AbstractFeedItemViewModel;
            vm?.Select(true);
            _lastSelected = vm;
        }
   }
}
