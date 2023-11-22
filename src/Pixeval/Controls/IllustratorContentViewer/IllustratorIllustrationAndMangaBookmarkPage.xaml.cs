#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorIllustrationAndMangaBookmarkPage.xaml.cs
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

using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Controls.IllustrationView;
using Pixeval.Util;
using Pixeval.Util.Threading;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustratorContentViewer;

public sealed partial class IllustratorIllustrationAndMangaBookmarkPage : ISortedIllustrationContainerPageHelper, IIllustratorContentViewerCommandBarHostSubPage
{
    private readonly IllustratorIllustrationAndMangaBookmarkPageViewModel _viewModel;

    private string? _uid;

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public IllustratorIllustrationAndMangaBookmarkPage()
    {
        InitializeComponent();
        _viewModel = new IllustratorIllustrationAndMangaBookmarkPageViewModel();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (ActivationCount > 1)
            return;

        _ = WeakReferenceMessenger.Default.TryRegister<IllustratorIllustrationAndMangaBookmarkPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
        if (e.Parameter is string id)
        {
            _uid = id;
            IllustrationContainer.IllustrationView.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.Bookmarks(id, PrivacyPolicy.Public, App.AppViewModel.AppSetting.TargetFilter)).Discard();
            _viewModel.LoadUserBookmarkTagsAsync(id).Discard();
            _viewModel.TagBookmarksIncrementallyLoaded += ViewModelOnTagBookmarksIncrementallyLoaded;
        }

        if (!App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer)
        {
            ChangeCommandBarVisibility(false);
        }
    }

    private void ViewModelOnTagBookmarksIncrementallyLoaded(object? sender, string e)
    {
        if (TagComboBox.SelectedItem is CountedTag(var (name, _), _) && name == e)
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = o => BookmarkTagFilter(name, o);
        }
    }

    private void TagComboBox_OnSelectionChangedWhenLoaded(object? sender, SelectionChangedEventArgs e)
    {
        if (TagComboBox.SelectedItem is CountedTag(var (name, _), _) tag && _uid is { Length: > 0 } id && !ReferenceEquals(tag, IllustratorIllustrationAndMangaBookmarkPageViewModel.EmptyCountedTag))
        {
            // fetch the bookmark IDs for tag, but do not wait for it.
            _viewModel.LoadBookmarksForTagAsync(id, tag.Tag.Name).Discard();

            // refresh the filter when there are newly fetched IDs.
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = o => BookmarkTagFilter(name, o);
            IllustrationContainer.IllustrationView.LoadMoreIfNeeded();
            return;
        }

        IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = null;
    }

    private bool BookmarkTagFilter(string name, object o) => o is IllustrationViewModel model && _viewModel.GetBookmarkIdsForTag(name).Contains(model.Id);

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }

    public void Dispose()
    {
        IllustrationContainer.IllustrationView.ViewModel.Dispose();
    }

    public void PerformSearch(string keyword)
    {
        if (IllustrationContainer.ShowCommandBar)
        {
            return;
        }

        if (keyword.IsNullOrBlank())
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = null;
        }
        else
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = o =>
            {
                if (o != null)
                {
                    return o.Id.Contains(keyword)
                           || (o.Illustrate.Tags ?? Enumerable.Empty<Tag>()).Any(x => x.Name.Contains(keyword) || (x.TranslatedName?.Contains(keyword) ?? false))
                           || (o.Illustrate.Title?.Contains(keyword) ?? false);
                }

                return false;
            };
        }
    }

    public void ChangeCommandBarVisibility(bool isVisible)
    {
        IllustrationContainer.ShowCommandBar = isVisible;
    }
}
