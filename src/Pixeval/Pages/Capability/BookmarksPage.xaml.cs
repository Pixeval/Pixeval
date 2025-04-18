// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.UI.Xaml;

namespace Pixeval.Pages.Capability;

public sealed partial class BookmarksPage : IScrollViewHost
{
    private BookmarksPageViewModel _viewModel = null!;

    public BookmarksPage()
    {
        InitializeComponent();
        SimpleWorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.SimpleWorkType;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _viewModel = new BookmarksPageViewModel(uid);
        _viewModel.TagBookmarksIncrementallyLoaded += ViewModelOnTagBookmarksIncrementallyLoaded;
        ChangeSource();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

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

    private bool BookmarkTagFilter(string name, IWorkViewModel viewModel) => viewModel.Entry is IWorkEntry entry && _viewModel.ContainsTag(name, entry.Id);

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

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
