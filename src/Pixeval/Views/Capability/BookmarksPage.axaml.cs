// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;

namespace Pixeval.Views.Capability;

public partial class BookmarksPage : ContentPage
{
    private readonly UserBasicInfo _user;
    private bool _suppressChangeSource;

    public static IReadOnlyList<BookmarkTag> DefaultTags { get; } = [AllBookmarkTag.Instance];

    public BookmarksPage() : this(App.AppViewModel.MakoClient.Me!)
    {
    }

    public BookmarksPage(UserBasicInfo user)
    {
        InitializeComponent();

        _user = user;
        if (_user.Id != App.AppViewModel.PixivUid)
            PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;

        FetchTags();
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
        TagComboBox.SelectedIndex = 0;
        _suppressChangeSource = false;
    }

    private void ChangeSource()
    {
        var tag = (TagComboBox.SelectedItem as BookmarkTag)?.Name;
        var workType = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>();
        var engine = App.AppViewModel.MakoClient.WorkBookmarks(
            _user.Id,
            workType,
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>(),
            tag);
        WorkContainer.ResetEngine(engine);
        App.AppViewModel.QueueWorkSubscriptionSyncCurrentSource(
            _user.Id,
            WorkSubscriptionType.Bookmarks,
            workType is SimpleWorkType.Novel
                ? WorkSubscriptionWorkKind.Novel
                : WorkSubscriptionWorkKind.IllustrationAndManga,
            engine);
    }

    private void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var workKind = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.Novel
            ? WorkSubscriptionWorkKind.Novel
            : WorkSubscriptionWorkKind.IllustrationAndManga;

        if (WorkSubscriptionHelper.TryAddOrUpdate(_user, WorkSubscriptionType.Bookmarks, workKind))
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.SubscriptionAdded));
    }
}
