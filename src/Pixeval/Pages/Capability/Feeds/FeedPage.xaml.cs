// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;
using Pixeval.Utilities;
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
                        FeedPageFrame.Navigate(typeof(CondensedFeedPage), illustrations, new CommonNavigationTransitionInfo());
                        break;
                    case FeedType.AddNovelBookmark:
                        IEnumerable<Novel> novels = await _viewModel.PerformLoadAsync(() =>
                            Task.WhenAll(entries.Select(entry => App.AppViewModel.MakoClient.GetNovelFromIdAsync(entry!.Id))));
                        FeedPageFrame.Navigate(typeof(CondensedFeedPage), novels, new CommonNavigationTransitionInfo());
                        break;
                    case FeedType.AddFavorite:
                        break;
                }

                break;
        }
    }
}
