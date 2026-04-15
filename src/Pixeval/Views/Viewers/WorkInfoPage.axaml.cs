// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Viewers;

public partial class WorkInfoPage : ContentPage
{
    public WorkInfoPage() : this(null)
    {
    }

    public WorkInfoPage(IArtworkInfo? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private async void AuthorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: IUser user })
            return;
        if (TopLevel.GetTopLevel(this) is not { Launcher: { } launcher, ViewContainer: { } viewContainer })
            return;

        if (user is UserInfo info)
            await viewContainer.CreateUserPageAsync(info.Id);
        else
            await launcher.LaunchUriAsync(user.WebsiteUri);
    }

    private void WorkTagButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IWorkEntry entry)
            return;
        if (sender is not Control { DataContext: ITag tag })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        var type = entry is Illustration ? SimpleWorkType.IllustrationAndManga : SimpleWorkType.Novel;
        SearchHistoryPersistentManager.AddHistory(tag.Name, tag.TranslatedName);
        viewContainer.NavigateTo(new SearchWorksPage(type, tag.Name));
    }

    private void BlockTag_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: ITag tag })
            return;
        var blockedTags = App.AppViewModel.AppSettings.BlockedTags;
        if (!blockedTags.Contains(tag.Name))
        {
            blockedTags.Add(tag.Name);
            AppInfo.SaveSettings(App.AppViewModel.AppSettings);
        }
    }
}
