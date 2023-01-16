using System;
using CommunityToolkit.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Misc;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.IllustratorContentViewer;

[DependencyProperty<IllustratorContentViewerViewModel>("ViewModel", nameof(OnViewModelChanged), IsSetterPublic = true)]
public sealed partial class IllustratorContentViewer
{
    private NavigationViewTag? _lastNavigationViewTag;

    public IllustratorContentViewer()
    {
        InitializeComponent();
    }

    private static async void OnViewModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is IllustratorContentViewer viewer && args.NewValue is IllustratorContentViewerViewModel viewModel)
        {
            await ThreadingHelper.SpinWaitAsync(() => viewModel.IllustrationTag is null);
            viewer.IllustratorContentViewerNavigationView.SelectedItem = viewer.IllustrationTab;
        }
    }

    private void IllustrationContentViewerNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
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
    }
}