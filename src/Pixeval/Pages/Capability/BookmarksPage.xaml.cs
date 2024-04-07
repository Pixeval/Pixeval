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

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Pages.Capability;

public sealed partial class BookmarksPage : IScrollViewProvider
{
    private BookmarkPageViewModel _viewModel = null!;

    public BookmarksPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _viewModel = new BookmarkPageViewModel(uid);
        ChangeSource();
        _viewModel.TagBookmarksIncrementallyLoaded += ViewModelOnTagBookmarksIncrementallyLoaded;
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void ViewModelOnTagBookmarksIncrementallyLoaded(object? sender, string e)
    {
        if (TagComboBox.SelectedItem is BookmarkTag { Name: var name } && name == e)
        {
            SetFilter(o => BookmarkTagFilter(name, o));
        }
    }

    private void TagComboBox_OnSelectionChangedWhenLoaded(object? sender, SelectionChangedEventArgs e)
    {
        if (TagComboBox.SelectedItem is BookmarkTag { Name: var name } tag && name != BookmarkTag.AllCountedTagString)
        {
            // fetch the bookmark IDs for tag, but do not wait for it.
            _ = _viewModel.LoadBookmarksForTagAsync(name, GetBookmarksEngine(name));

            // refresh the filter when there are newly fetched IDs.
            SetFilter(o => BookmarkTagFilter(name, o));
            return;
        }

        SetFilter(null);
    }

    private bool BookmarkTagFilter(string name, IWorkViewModel viewModel) => _viewModel.ContainsTag(name, viewModel.Id);

    private async void ChangeSource()
    {
        var policy = GetPolicy();
        var engine = GetBookmarksEngine(null);

        SetFilter(null);
        WorkContainer.WorkView.ResetEngine(engine);
        var source = await _viewModel.SetBookmarkTagsAsync(policy, SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>());
        TagComboBox.ItemsSource = source;
        TagComboBox.SelectedItem = source[0];
    }

    private IFetchEngine<IWorkEntry> GetBookmarksEngine(string? tag)
    {
        var policy = GetPolicy();
        return SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga
            ? App.AppViewModel.MakoClient.IllustrationBookmarks(_viewModel.UserId, policy, tag, App.AppViewModel.AppSettings.TargetFilter)
            : App.AppViewModel.MakoClient.NovelBookmarks(_viewModel.UserId, policy, tag, App.AppViewModel.AppSettings.TargetFilter);
    }

    private PrivacyPolicy GetPolicy()
    {
        var policy = PrivacyPolicyComboBox.GetSelectedItem<PrivacyPolicy>();
        if (policy is PrivacyPolicy.Private && !_viewModel.IsMe)
            policy = PrivacyPolicy.Public;
        return policy;
    }

    private void SetFilter(Func<IWorkViewModel, bool>? filter)
    {
        if (WorkContainer.ViewModel is { } vm)
            vm.Filter = filter;
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
