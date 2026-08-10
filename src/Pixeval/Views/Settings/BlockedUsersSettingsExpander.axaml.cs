// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Settings.Entries;
using Pixeval.Utilities;
using Pixeval.ViewModels.Settings;

namespace Pixeval.Views.Settings;

public partial class BlockedUsersSettingsExpander : SettingsExpander, IEntryControl<BlockedUsersSettingsEntry>
{
    private CancellationTokenSource? _reloadCancellationTokenSource;

    public ObservableCollection<BlockedUserItemViewModel> Users { get; } = [];

    public BlockedUsersSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            _ = ReloadAsync();
        }
    }

    public BlockedUsersSettingsExpander() => InitializeComponent();

    private static BlockedUserPersistentManager UserManager =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<BlockedUserPersistentManager>();

    private async Task ReloadAsync()
    {
        _reloadCancellationTokenSource?.Cancel();
        _reloadCancellationTokenSource?.Dispose();
        _reloadCancellationTokenSource = new();
        var token = _reloadCancellationTokenSource.Token;
        Users.Clear();
        try
        {
            await foreach (var entry in UserManager.StreamEntriesAsync(token: token))
                Users.Add(new(entry));
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
        HideError();
        if (!long.TryParse(TargetIdTextBox.Text, out var userId) || userId <= 0)
        {
            ShowError(I18NManager.GetResource(BlockedUsersSettingsExpanderResources.InvalidUserId));
            return;
        }

        try
        {
            var user = (await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId)).UserEntity;
            UserManager.Upsert(BlockedContentModelHelper.CreateBlockedUserEntry(user));
            TargetIdTextBox.Text = "";
            await ReloadAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async void RefreshButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BlockedUserItemViewModel item })
            await RefreshUserAsync(item);
    }

    private async void RefreshAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        HideError();
        foreach (var item in Users.ToArray())
            await RefreshUserAsync(item);
    }

    private async Task RefreshUserAsync(BlockedUserItemViewModel item)
    {
        try
        {
            var user = (await App.AppViewModel.MakoClient.GetUserFromIdAsync(item.Entry.Id)).UserEntity;
            var entry = UserManager.Upsert(BlockedContentModelHelper.CreateBlockedUserEntry(user));
            item.UpdateUser(entry);
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async void DeleteButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: BlockedUserItemViewModel item })
            return;

        _ = UserManager.TryDeleteByUserId(item.Entry.Id);
        await ReloadAsync();
    }

    private void ShowError(string text)
    {
        ErrorTextBlock.Text = text;
        ErrorTextBlock.IsVisible = true;
    }

    private void HideError() => ErrorTextBlock.IsVisible = false;

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _reloadCancellationTokenSource?.Cancel();
        _reloadCancellationTokenSource?.Dispose();
        _reloadCancellationTokenSource = null;
        base.OnUnloaded(e);
    }
}
