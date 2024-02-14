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

using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls.IllustratorContentViewer;

public sealed partial class IllustratorIllustrationAndMangaBookmarkPage
{
    private readonly IllustratorIllustrationAndMangaBookmarkPageViewModel _viewModel = new();

    private long _uid;

    public IllustratorIllustrationAndMangaBookmarkPage() => InitializeComponent();

    public override IllustrationContainer ViewModelProvider => IllustrationContainer;

    public override SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageActivated(long id)
    {
        _uid = id;
        IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Bookmarks(id, PrivacyPolicy.Public, App.AppViewModel.AppSettings.TargetFilter));
        _ = _viewModel.LoadUserBookmarkTagsAsync(id);
        _viewModel.TagBookmarksIncrementallyLoaded += ViewModelOnTagBookmarksIncrementallyLoaded;
    }

    private void ViewModelOnTagBookmarksIncrementallyLoaded(object? sender, string e)
    {
        if (TagComboBox.SelectedItem is CountedTag({ Name: var name }, _) && name == e)
        {
            IllustrationContainer.ViewModel.DataProvider.View.Filter = o => BookmarkTagFilter(name, o);
        }
    }

    private void TagComboBox_OnSelectionChangedWhenLoaded(object? sender, SelectionChangedEventArgs e)
    {
        if (TagComboBox.SelectedItem is CountedTag({ Name: var name }, _) tag && !ReferenceEquals(tag, IllustratorIllustrationAndMangaBookmarkPageViewModel.EmptyCountedTag))
        {
            // fetch the bookmark IDs for tag, but do not wait for it.
            _ = _viewModel.LoadBookmarksForTagAsync(_uid, tag.Tag.Name);

            // refresh the filter when there are newly fetched IDs.
            IllustrationContainer.ViewModel.DataProvider.View.Filter = o => BookmarkTagFilter(name, o);
            return;
        }

        IllustrationContainer.ViewModel.DataProvider.View.Filter = null;
    }

    private bool BookmarkTagFilter(string name, object o) => o is IllustrationItemViewModel model && _viewModel.GetBookmarkIdsForTag(name).Contains(model.Id);
}
