// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Avalonia.Controls;
using Mako.Global.Enum;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Models.Settings.Entries;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels.Settings;

namespace Pixeval.Views.Settings;

public partial class WorkSubscriptionsSettingsExpander : SettingsExpander, IEntryControl<WorkSubscriptionsSettingsEntry>
{
    private CancellationTokenSource? _reloadCancellationTokenSource;

    public ObservableCollection<WorkSubscriptionItemViewModel> Subscriptions { get; } = [];

    public WorkSubscriptionsSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            _ = ReloadAsync();
        }
    }

    public WorkSubscriptionsSettingsExpander()
    {
        InitializeComponent();
        UpdateWorkKindItems();
    }

    private static WorkSubscriptionPersistentManager SubscriptionManager =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();

    private static IReadOnlyList<SymbolComboBoxItem> BookmarkWorkKinds { get; } =
        [.. SymbolComboBoxItem.GetValues<WorkSubscriptionWorkKind>().Where(t => t.Value is WorkSubscriptionWorkKind.Illustration or WorkSubscriptionWorkKind.Novel)];

    private static IReadOnlyList<SymbolComboBoxItem> PostWorkKinds { get; } =
        [.. SymbolComboBoxItem.GetValues<WorkSubscriptionWorkKind>().Where(t => t.Value is WorkSubscriptionWorkKind.Illustration or WorkSubscriptionWorkKind.Manga or WorkSubscriptionWorkKind.Novel)];

    private static IReadOnlyList<SymbolComboBoxItem> SeriesWorkKinds => BookmarkWorkKinds;

    private async Task ReloadAsync()
    {
        _reloadCancellationTokenSource?.Cancel();
        _reloadCancellationTokenSource?.Dispose();
        _reloadCancellationTokenSource = new();
        var token = _reloadCancellationTokenSource.Token;
        Subscriptions.Clear();
        try
        {
            await foreach (var entry in SubscriptionManager.StreamEntriesAsync(token: token))
                Subscriptions.Add(new(entry));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(ReloadAsync), e);
        }
    }

    private async void AddButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ErrorTextBlock.IsVisible = false;
        if (!long.TryParse(TargetIdTextBox.Text, out var targetId) || targetId <= 0)
        {
            ShowError(I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.InvalidTargetId));
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

        if (subscriptionType is WorkSubscriptionType.Series)
        {
            var simpleWorkType = workKind is WorkSubscriptionWorkKind.Novel
                ? SimpleWorkType.Novel
                : SimpleWorkType.Illustration;
            var (detail, first, _) = await App.AppViewModel.MakoClient.GetWorkSeriesAsync(simpleWorkType, targetId);
            _ = WorkSubscriptionHelper.TryAddOrUpdateSeries(targetId, workKind, detail, first);
        }
        else
        {
            var user = (await App.AppViewModel.MakoClient.GetUserFromIdAsync(targetId)).UserEntity;
            _ = WorkSubscriptionHelper.TryAddOrUpdate(user, subscriptionType, workKind);
        }

        TargetIdTextBox.Text = "";
        await ReloadAsync();
    }

    private async void DeleteButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: WorkSubscriptionItemViewModel item })
            return;

        _ = SubscriptionManager.TryDelete(item.Entry);
        await ReloadAsync();
    }

    private void SyncAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        App.AppViewModel.QueueWorkSubscriptionSyncAll();
    }

    private void SubscriptionTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        UpdateWorkKindItems();
    }

    private void UpdateWorkKindItems()
    {
        var items = SubscriptionTypeComboBox.GetSelectedValue<WorkSubscriptionType>() switch
        {
            WorkSubscriptionType.Bookmarks => BookmarkWorkKinds,
            WorkSubscriptionType.Posts => PostWorkKinds,
            WorkSubscriptionType.Series => SeriesWorkKinds,
            _ => throw new ArgumentOutOfRangeException()
        };
        WorkKindComboBox.ItemsSource = items;
        if (items is not [])
            WorkKindComboBox.SelectedValue = items[0].Value;
    }

    private void ShowError(string text)
    {
        ErrorTextBlock.Text = text;
        ErrorTextBlock.IsVisible = true;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = ReloadAsync();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _reloadCancellationTokenSource?.Cancel();
        _reloadCancellationTokenSource?.Dispose();
        _reloadCancellationTokenSource = null;
        base.OnUnloaded(e);
    }
}
