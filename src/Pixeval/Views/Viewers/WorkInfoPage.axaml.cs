// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Utilities;

namespace Pixeval.Views.Viewers;

public partial class WorkInfoPage : ContentPage
{
    public WorkInfoPage() => InitializeComponent();

    private async void AuthorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: IIdEntry { Id: var id } })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            await viewContainer.CreateUserPageAsync(id);
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
