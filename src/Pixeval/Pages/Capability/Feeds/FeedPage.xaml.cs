// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;
using Pixeval.Utilities;
using Windows.UI.Text;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability.Feeds;

public sealed partial class FeedPage
{
    private AbstractFeedItemViewModel? _lastSelected;

    private readonly FeedPageViewModel _viewModel;

    public FeedPage()
    {
        InitializeComponent();
        _viewModel = new FeedPageViewModel();
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

        contentTextBlock?.Inlines.Clear();
        switch (entry.Type)
        {
            case FeedType.AddBookmark or FeedType.AddNovelBookmark:
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparseBookmarkPrefix : _CondensedBookmarkPrefix });
                contentTextBlock?.Inlines.Add(feedNameString);
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparseBookmarkSuffix : _CondensedBookmarkFormattedSuffix.Format(entriesLengthIfCondensed) });
                break;
            case FeedType.PostIllust:
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparsePostIllustPrefix : _CondensedPostIllustPrefix });
                contentTextBlock?.Inlines.Add(feedNameString);
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparsePostIllustSuffix : _CondensedPostIllustFormattedSuffix.Format(entriesLengthIfCondensed) });
                break;
            case FeedType.AddFavorite:
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparseFollowUserPrefix : _CondensedFollowUserIllustPrefix });
                contentTextBlock?.Inlines.Add(feedNameString);
                contentTextBlock?.Inlines.Add(new Run { Text = isSparse ? _SparseFollowUserSuffix : _CondensedFollowUserFormattedSuffix.Format(entriesLengthIfCondensed) });
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

    private async void ItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        _lastSelected?.Select(false);
        var vm = sender.SelectedItem as AbstractFeedItemViewModel;
        vm?.Select(true);
        _lastSelected = vm;

        _viewModel.CancelLoad();

        switch (vm)
        {
            case FeedItemSparseViewModel { Entry: IFeedEntry.SparseFeedEntry svmEntry }:
                switch (svmEntry.Entry.Type)
                {
                    case FeedType.AddBookmark or FeedType.PostIllust:
                        var illustration = await _viewModel.PerformLoadAsync(() => App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(svmEntry.Id));
                        var illustrationViewer = new IllustrationViewerPage();
                        illustrationViewer.SetViewModel((new List<IllustrationItemViewModel> { new(illustration) }, 0));
                        FeedPageFrame.Content = illustrationViewer;
                        break;
                    case FeedType.AddNovelBookmark:
                        var novel = await _viewModel.PerformLoadAsync(() => App.AppViewModel.MakoClient.GetNovelFromIdAsync(svmEntry.Id));
                        var novelItemView = new NovelViewerPage();
                        novelItemView.SetViewModel((new List<NovelItemViewModel> { new(novel) }, 0));
                        FeedPageFrame.Content = novelItemView;
                        break;
                    case FeedType.AddFavorite:
                        var user = await _viewModel.PerformLoadAsync(() => App.AppViewModel.MakoClient.GetUserFromIdAsync(svmEntry.Id, App.AppViewModel.AppSettings.TargetFilter));
                        var illustratorViewer = new IllustratorViewerPage();
                        illustratorViewer.SetViewModel(user);
                        FeedPageFrame.Content = illustratorViewer;
                        break;
                }

                break;
            case FeedItemCondensedViewModel { Entry: IFeedEntry.CondensedFeedEntry(var entries) }:
                switch (vm.GetMostSignificantEntry()!.Type)
                {
                    case FeedType.AddBookmark or FeedType.PostIllust:
                        IEnumerable<IWorkEntry> illustrations = await _viewModel.PerformLoadAsync(() =>
                            Task.WhenAll(entries.Select(entry => App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(entry!.Id))));
                        _ = FeedPageFrame.Navigate(typeof(CondensedFeedPage), illustrations, new CommonNavigationTransitionInfo());
                        break;
                    case FeedType.AddNovelBookmark:
                        IEnumerable<Novel> novels = await _viewModel.PerformLoadAsync(() =>
                            Task.WhenAll(entries.Select(entry => App.AppViewModel.MakoClient.GetNovelFromIdAsync(entry!.Id))));
                        _ = FeedPageFrame.Navigate(typeof(CondensedFeedPage), novels, new CommonNavigationTransitionInfo());
                        break;
                    case FeedType.AddFavorite:
                        break;
                }

                break;
        }
    }

    private static readonly string _SparseBookmarkPrefix;
    private static readonly string _SparseBookmarkSuffix;
    private static readonly string _SparsePostIllustPrefix;
    private static readonly string _SparsePostIllustSuffix;
    private static readonly string _SparseFollowUserPrefix;
    private static readonly string _SparseFollowUserSuffix;
    private static readonly string _CondensedBookmarkPrefix;
    private static readonly string _CondensedBookmarkFormattedSuffix;
    private static readonly string _CondensedPostIllustPrefix;
    private static readonly string _CondensedPostIllustFormattedSuffix;
    private static readonly string _CondensedFollowUserIllustPrefix;
    private static readonly string _CondensedFollowUserFormattedSuffix;

    static FeedPage()
    {
        var sparseBookmark = FeedPageResources.SparseBookmarkFormatted.Split("{0}");
        _SparseBookmarkPrefix = sparseBookmark[0];
        _SparseBookmarkSuffix = sparseBookmark[1];
        var sparsePostIllust = FeedPageResources.SparsePostIllustFormatted.Split("{0}");
        _SparsePostIllustPrefix = sparsePostIllust[0];
        _SparsePostIllustSuffix = sparsePostIllust[1];
        var sparseFollowUser = FeedPageResources.SparseFollowUserFormatted.Split("{0}");
        _SparseFollowUserPrefix = sparseFollowUser[0];
        _SparseFollowUserSuffix = sparseFollowUser[1];
        var condensedBookmark = FeedPageResources.CondensedBookmarkFormatted.Split("{0}");
        _CondensedBookmarkPrefix = condensedBookmark[0];
        _CondensedBookmarkFormattedSuffix = condensedBookmark[1];
        var condensedPostIllust = FeedPageResources.CondensedPostIllustFormatted.Split("{0}");
        _CondensedPostIllustPrefix = condensedPostIllust[0];
        _CondensedPostIllustFormattedSuffix = condensedPostIllust[1];
        var condensedFollowUser = FeedPageResources.CondensedFollowUserFormatted.Split("{0}");
        _CondensedFollowUserIllustPrefix = condensedFollowUser[0];
        _CondensedFollowUserFormattedSuffix = condensedFollowUser[1];
    }
}
