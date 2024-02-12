#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorContentViewer.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.Misc;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls.IllustratorContentViewer;

public sealed partial class IllustratorContentViewer : IDisposable
{
    private readonly ISet<Page> _pageCache;
    private NavigationViewTag? _lastNavigationViewTag;

    public IllustratorContentViewerViewModel ViewModel { get; set; } = null!;

    public IllustratorContentViewer()
    {
        InitializeComponent();
        _pageCache = new HashSet<Page>();
    }

    public void Dispose()
    {
        _pageCache.OfType<IDisposable>().ForEach(p => p.Dispose());
        _pageCache.Clear();
    }

    private async void IllustrationContentViewerNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (App.AppViewModel.AppSetting.ShowRecommendIllustratorsInIllustratorContentViewer)
        {
            _ = ViewModel.LoadRecommendIllustratorsAsync();
        }

        ViewModel.ShowExternalCommandBarChanged += (_, b) =>
        {
            if (IllustratorContentViewerFrame.Content is IllustratorContentViewerSubPage subPage)
                subPage.ViewModelProvider.ShowCommandBar = b;
        };

        ViewModel.ShowRecommendIllustratorsChanged += (o, b) =>
        {
            if (b && ViewModel.RecommendIllustrators.Count is 0)
                _ = ViewModel.LoadRecommendIllustratorsAsync();
        };
        var currentTag = args.SelectedItemContainer.GetTag<NavigationViewTag>();
        ViewModel.CurrentTab = currentTag switch
        {
            _ when ReferenceEquals(currentTag, ViewModel.IllustrationTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Illustration,
            _ when ReferenceEquals(currentTag, ViewModel.MangaTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Manga,
            _ when ReferenceEquals(currentTag, ViewModel.NovelTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.Novel,
            _ when ReferenceEquals(currentTag, ViewModel.BookmarkedIllustrationAndMangaTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.BookmarkedIllustrationAndManga,
            _ when ReferenceEquals(currentTag, ViewModel.BookmarkedNovelTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.BookmarkedNovel,
            _ when ReferenceEquals(currentTag, ViewModel.FollowingUserTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.FollowingUser,
            _ when ReferenceEquals(currentTag, ViewModel.MyPixivUserTag) => IllustratorContentViewerViewModel.IllustratorContentViewerTab.MyPixivUser,
            _ => ThrowHelper.ArgumentOutOfRange<object, IllustratorContentViewerViewModel.IllustratorContentViewerTab>(args.SelectedItemContainer.Tag)
        };
        IllustratorContentViewerFrame.NavigateByNavigationViewTag(sender, _lastNavigationViewTag is var (_, _, index) && args.SelectedItemContainer.Tag is NavigationViewTag(_, _, var currentIndex)
            ? index > currentIndex ? new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft } : new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
            : new EntranceNavigationTransitionInfo());
        _lastNavigationViewTag = currentTag;

        await ThreadingHelper.SpinWaitAsync(() => IllustratorContentViewerFrame.Content?.GetType() != _lastNavigationViewTag.NavigateTo);
        _ = _pageCache.Add((Page)IllustratorContentViewerFrame.Content);
    }

    private void NavigationViewAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (IllustratorContentViewerFrame.Content is IllustratorContentViewerSubPage subPage)
        {
            subPage.PerformSearch(sender.Text);
        }
    }

    private async void IllustratorItem_OnViewModelChanged(RecommendIllustratorItem item, RecommendIllustratorItemViewModel viewModel)
    {
        await viewModel.LoadAvatarAsync();
    }

    private async void ItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (Parent is Page { Frame: { } frame })
            if (sender.SelectedItem is RecommendIllustratorItemViewModel viewModel)
            {
                var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(viewModel.UserId, App.AppViewModel.AppSetting.TargetFilter);
                _ = frame.Navigate(typeof(IllustratorContentViewerPage), userDetail);
            }
    }
}
