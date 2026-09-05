// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Work;

namespace Pixeval.Views.Capability;

public partial class WorkBookmarksPage : IconContentPage
{
    private readonly UserBasicInfo _user;
    private readonly string? _initialTag;
    private bool _suppressChangeSource;

    public static IReadOnlyList<BookmarkTag> DefaultTags { get; } = [AllBookmarkTag.Instance, UncategorizedBookmarkTag.Instance];

    private static IWorkSubscriptionService SubscriptionService =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<IWorkSubscriptionService>();

    public WorkBookmarksPage() : this(PixevalSettings.Me)
    {
    }

    public WorkBookmarksPage(UserBasicInfo user, SimpleWorkType simpleWorkType = SimpleWorkType.Illustration, PrivacyPolicy privacyPolicy = PrivacyPolicy.Public, string? tag = null, IWorkViewViewModel? viewModel = null)
    {
        InitializeComponent();

        _user = user;
        _initialTag = tag;
        SimpleWorkTypeComboBox.SelectedValue = simpleWorkType;
        PrivacyPolicyComboBox.SelectedValue = privacyPolicy;
        if (_user.Id != PixevalSettings.MyId)
            PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;

        FetchTags();
        if (viewModel is not null)
        {
            WorkContainer.SetViewModel(viewModel);
            UpdateSubscriptionButtons();
        }
        else
            ChangeSource();
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        FetchTags();
        ChangeSource();
    }

    private void TagComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _suppressChangeSource)
            return;

        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    public async void FetchTags()
    {
        var tags = await MakoHelper.GetBookmarkTagsAsync(
            _user.Id,
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>());

        _suppressChangeSource = true;
        TagComboBox.ItemsSource = tags;
        TagComboBox.SelectedItem = tags.FirstOrDefault(tag => tag.Name == _initialTag) ?? AllBookmarkTag.Instance;
        _suppressChangeSource = false;
    }

    private void ChangeSource()
    {
        var tag = (TagComboBox.SelectedItem as BookmarkTag)?.Name;
        var workType = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>();
        var engine = App.AppViewModel.MakoClient.WorkBookmarks(
            workType,
            _user.Id,
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>(),
            tag);
        WorkContainer.ResetEngine(engine);
        App.AppViewModel.QueueWorkSubscriptionSyncCurrentSource(
            _user.Id,
            WorkSubscriptionType.Bookmarks,
            GetSubscriptionWorkKind(),
            engine);
        UpdateSubscriptionButtons();
    }

    private async void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var workKind = GetSubscriptionWorkKind();

        _ = await WorkSubscriptionButtonHelper.RunAsync(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            () => Task.FromResult(WorkSubscriptionHelper.TryAddOrUpdateUser(_user, WorkSubscriptionType.Bookmarks, workKind)),
            UpdateSubscriptionButtons);
    }

    private async void RemoveSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        _ = await WorkSubscriptionButtonHelper.RunAsync(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            RemoveCurrentSubscriptionAsync,
            UpdateSubscriptionButtons);
    }

    private void UpdateSubscriptionButtons() =>
        WorkSubscriptionButtonHelper.UpdateVisibility(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            SubscriptionService.TryGetSubscription(_user.Id, WorkSubscriptionType.Bookmarks, GetSubscriptionWorkKind()) is not null);

    private async Task<bool> RemoveCurrentSubscriptionAsync()
    {
        if (SubscriptionService.TryGetSubscription(_user.Id, WorkSubscriptionType.Bookmarks, GetSubscriptionWorkKind()) is not { HistoryEntryId: var historyEntryId })
            return false;

        _ = await SubscriptionService.TryRemoveAsync(historyEntryId);
        return true;
    }

    private WorkSubscriptionWorkKind GetSubscriptionWorkKind() =>
        SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.Novel
            ? WorkSubscriptionWorkKind.Novel
            : WorkSubscriptionWorkKind.Illustration;
}
