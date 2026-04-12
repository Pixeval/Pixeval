// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Misaki;
using Pixeval.AppManagement;

namespace Pixeval.Views.Viewers;

public partial class WorkInfoPage : ContentPage
{
    public WorkInfoPage() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not IArtworkInfo entry)
            return;

        var hasView = entry.TotalView > 0;
        var hasFav = entry.TotalFavorite > 0;
        ViewCountIconText.IsVisible = hasView;
        FavoriteCountIconText.IsVisible = hasFav;
        CountsPanel.IsVisible = hasView || hasFav;

        CreateDateIconText.IsVisible = entry.CreateDate != default;

        var hasAuthors = entry.Authors.Count > 0;
        var hasUploaders = entry.Uploaders.Count > 0;
        AuthorsRepeater.IsVisible = hasAuthors;
        UploadersRepeater.IsVisible = hasUploaders;
    }

    private async void CopyIdButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IArtworkInfo entry && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(entry.Id);
    }

    private async void CopyUserIdButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: { } id } && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(id.ToString());
    }

    private void AuthorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Navigate to user page
    }

    private void WorkTagButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        // TODO: Navigate to tag search
    }

    private async void CopyTagName_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: string text } && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
    }

    private void BlockTag_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: ITag tag })
            return;
        var blockedTags = App.AppViewModel.AppSettings.BlockedTags;
        if (!blockedTags.Contains(tag.Name))
        {
            blockedTags.Add(tag.Name);
            AppInfo.SaveSettings(App.AppViewModel.AppSettings);
        }
    }
}
