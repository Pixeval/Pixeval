// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Models.Settings.Entries;
using Pixeval.ViewModels.Settings;

namespace Pixeval.Views.Settings;

public partial class WorkSubscriptionsSettingsExpander : SettingsExpander, IEntryControl<WorkSubscriptionsSettingsEntry>
{
    public ObservableCollection<WorkSubscriptionItemViewModel> Subscriptions { get; } = [];

    public WorkSubscriptionsSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            Reload();
        }
    }

    public WorkSubscriptionsSettingsExpander()
    {
        InitializeComponent();
        SubscriptionListBox.ItemsSource = Subscriptions;
        UpdateWorkKindItems();
    }

    private static WorkSubscriptionPersistentManager SubscriptionManager =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();

    private static DownloadFolderPersistentManager FolderManager =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadFolderPersistentManager>();

    private static IReadOnlyList<SymbolComboBoxItem> BookmarkWorkKinds { get; } =
        [.. SymbolComboBoxItem.GetValues<WorkSubscriptionWorkKind>().Where(t => t.Value is WorkSubscriptionWorkKind.IllustrationAndManga or WorkSubscriptionWorkKind.Novel)];

    private static IReadOnlyList<SymbolComboBoxItem> PostWorkKinds { get; } =
        [.. SymbolComboBoxItem.GetValues<WorkSubscriptionWorkKind>().Where(t => t.Value is WorkSubscriptionWorkKind.Illustration or WorkSubscriptionWorkKind.Manga or WorkSubscriptionWorkKind.Novel)];

    private void Reload()
    {
        Subscriptions.Clear();
        foreach (var entry in SubscriptionManager.Reverse())
            Subscriptions.Add(new(entry));
    }

    private async void AddButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ErrorTextBlock.IsVisible = false;
        if (!long.TryParse(UserIdTextBox.Text, out var userId) || userId <= 0)
        {
            ShowError(I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.InvalidUserId));
            return;
        }

        var subscriptionType = SubscriptionTypeComboBox.GetSelectedValue<WorkSubscriptionType>();
        if (WorkKindComboBox.SelectedValue is not WorkSubscriptionWorkKind workKind)
        {
            UpdateWorkKindItems();
            if (WorkKindComboBox.SelectedValue is not WorkSubscriptionWorkKind selectedWorkKind)
                return;

            workKind = selectedWorkKind;
        }
        var name = "";
        try
        {
            name = (await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId)).UserEntity.Name;
        }
        catch
        {
            // Sync can fill the name from the author field later.
        }

        var entry = SubscriptionManager.Upsert(new()
        {
            UserId = userId,
            SubscriptionType = subscriptionType,
            WorkKind = workKind,
            Name = name
        });
        _ = FolderManager.GetOrCreate(entry);
        UserIdTextBox.Text = "";
        Reload();
        App.AppViewModel.QueueWorkSubscriptionSyncAll();
    }

    private void DeleteButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: WorkSubscriptionItemViewModel item })
            return;

        _ = SubscriptionManager.TryDelete(item.Entry);
        Reload();
    }

    private void SubscriptionTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        UpdateWorkKindItems();
    }

    private void UpdateWorkKindItems()
    {
        var items = SubscriptionTypeComboBox.GetSelectedValue<WorkSubscriptionType>() is WorkSubscriptionType.Bookmarks
            ? BookmarkWorkKinds
            : PostWorkKinds;
        WorkKindComboBox.ItemsSource = items;
        if (items is not [])
            WorkKindComboBox.SelectedValue = items[0].Value;
    }

    private void ShowError(string text)
    {
        ErrorTextBlock.Text = text;
        ErrorTextBlock.IsVisible = true;
    }
}
