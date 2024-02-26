#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/BookmarksPage.xaml.cs
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
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Pages.Capability;

public sealed partial class BookmarksPage : IScrollViewProvider
{
    private BookmarkPageViewModel _viewModel = null!;

    public BookmarksPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _viewModel = new BookmarkPageViewModel(uid);
        ChangeSource();
        _viewModel.TagBookmarksIncrementallyLoaded += ViewModelOnTagBookmarksIncrementallyLoaded;
        await _viewModel.LoadUserBookmarkTagsAsync();
    }

    private void PrivacyPolicyComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
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
        if (TagComboBox.SelectedItem is CountedTag({ Name: var name }, _) tag && !ReferenceEquals(tag, BookmarkPageViewModel.EmptyCountedTag))
        {
            // fetch the bookmark IDs for tag, but do not wait for it.
            _ = _viewModel.LoadBookmarksForTagAsync(tag.Tag.Name);

            // refresh the filter when there are newly fetched IDs.
            IllustrationContainer.ViewModel.DataProvider.View.Filter = o => BookmarkTagFilter(name, o);
            return;
        }

        IllustrationContainer.ViewModel.DataProvider.View.Filter = null;
    }

    private bool BookmarkTagFilter(string name, object o) => o is IllustrationItemViewModel model && _viewModel.GetBookmarkIdsForTag(name).Contains(model.Id);

    private void ChangeSource()
    {
        var tag = PrivacyPolicyComboBox.SelectedItem;
        if (tag is PrivacyPolicy.Private && !_viewModel.IsMe)
            tag = PrivacyPolicy.Public;
        IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Bookmarks(_viewModel.UserId, tag, App.AppViewModel.AppSettings.TargetFilter));
    }

    public ScrollView ScrollView => IllustrationContainer.ScrollView;
}
