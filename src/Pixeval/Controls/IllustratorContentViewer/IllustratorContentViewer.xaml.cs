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
using CommunityToolkit.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Misc;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustratorContentViewer;

[DependencyProperty<IllustratorContentViewerViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class IllustratorContentViewer : IDisposable
{
    private readonly ISet<Page> _pageCache;
    private NavigationViewTag? _lastNavigationViewTag;

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
        if (IllustratorContentViewerFrame.Content is IllustratorContentViewerSubPage subPage)
        {
            subPage.PerformSearch(sender.Text);
        }
    }
}
