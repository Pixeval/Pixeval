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

    private void AuthorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Navigate to user page
    }

    private void WorkTagButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        // TODO: Navigate to tag search
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
