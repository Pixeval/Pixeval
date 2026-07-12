// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class WorkBookmarksPage : IconContentPage
{
    private readonly UserBasicInfo _user;
    private readonly string? _initialTag;
    private bool _suppressChangeSource;

    public static IReadOnlyList<BookmarkTag> DefaultTags { get; } = [AllBookmarkTag.Instance];

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
            WorkContainer.SetViewModel(viewModel);
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
                : WorkSubscriptionWorkKind.Illustration,
            engine);
    }

    private void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var workKind = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.Novel
            ? WorkSubscriptionWorkKind.Novel
            : WorkSubscriptionWorkKind.Illustration;

        if (WorkSubscriptionHelper.TryAddOrUpdate(_user, WorkSubscriptionType.Bookmarks, workKind))
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.SubscriptionAdded));
    }
}
