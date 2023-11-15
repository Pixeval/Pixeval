using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Misc;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.IllustratorContentViewer;

[DependencyProperty<IllustratorContentViewerViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class IllustratorContentViewer : IDisposable
{
    private NavigationViewTag? _lastNavigationViewTag;

    private readonly ISet<Page> _pageCache;

    public IllustratorContentViewer()
    {
        InitializeComponent();
        _pageCache = new HashSet<Page>();
    }

    private static async void OnViewModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is IllustratorContentViewer viewer && args.NewValue is IllustratorContentViewerViewModel viewModel)
        {
            await ThreadingHelper.SpinWaitAsync(() => viewModel.IllustrationTag is null);
            viewer.IllustratorContentViewerNavigationView.SelectedItem = viewer.IllustrationTab;
        }
    }

    private async void IllustrationContentViewerNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (App.AppViewModel.AppSetting.ShowRecommendIllustratorsInIllustratorContentViewer)
        {
            ViewModel.LoadRecommendIllustratorsAsync().Discard();
        }

        ViewModel.ShowExternalCommandBarChanged += (_, b) =>
        {
            if (IllustratorContentViewerFrame.Content is IIllustratorContentViewerCommandBarHostSubPage subPage)
                subPage.ChangeCommandBarVisibility(b);
        };

        ViewModel.ShowRecommendIllustratorsChanged += (_, b) =>
        {
            if (b && !ViewModel.RecommendIllustrators.Any())
                ViewModel.LoadRecommendIllustratorsAsync().Discard();
        };

        ViewModel.CurrentTab = args.SelectedItemContainer.Tag switch
        {
            var tag when ReferenceEquals(tag, ViewModel.IllustrationTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Illustration,
            var tag when ReferenceEquals(tag, ViewModel.MangaTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Manga,
            var tag when ReferenceEquals(tag, ViewModel.NovelTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Novel,
            var tag when ReferenceEquals(tag, ViewModel.BookmarkedIllustrationAndMangaTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.BookmarkedIllustrationAndManga,
            var tag when ReferenceEquals(tag, ViewModel.BookmarkedNovelTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.BookmarkedNovel,
            var tag when ReferenceEquals(tag, ViewModel.FollowingUserTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.FollowingUser,
            var tag when ReferenceEquals(tag, ViewModel.MyPixivUserTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.MyPixivUser,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<IllustratorContentViewerViewModel.IllustratorContentViewerTab>(nameof(sender.Tag))
        };
        IllustratorContentViewerFrame.NavigateByNavigationViewTag(sender, _lastNavigationViewTag is var (_, _, index) && args.SelectedItemContainer.Tag is NavigationViewTag(_, _, var currentIndex)
            ? index > currentIndex ? new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft } : new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
            : new EntranceNavigationTransitionInfo());
        _lastNavigationViewTag = args.SelectedItemContainer.Tag as NavigationViewTag;

        await ThreadingHelper.SpinWaitAsync(() => IllustratorContentViewerFrame.Content.GetType() != _lastNavigationViewTag!.NavigateTo);
        _ = _pageCache.Add((Page)IllustratorContentViewerFrame.Content);
    }

    private void NavigationViewAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (IllustratorContentViewerFrame.Content is IIllustratorContentViewerSubPage subPage)
        {
            subPage.PerformSearch(sender.Text);
        }
    }

    public void Dispose()
    {
        _pageCache.OfType<IDisposable>().ForEach(p => p.Dispose());
        _pageCache.Clear();
    }
}
