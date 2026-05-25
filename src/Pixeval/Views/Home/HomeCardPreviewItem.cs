// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Mako.Model;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.ViewContainers;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Home;

public abstract class HomeCardPreviewItem(Func<Control?, Task> openAsync)
{
    public ICommand OpenCommand { get; } = new AsyncRelayCommand<Control?>(openAsync);
}

public sealed class HomeCardImagePreviewItem(IArtworkInfo artworkInfo)
    : HomeCardPreviewItem(control => OpenAsync(control, artworkInfo))
{
    public IArtworkInfo ArtworkInfo { get; } = artworkInfo;

    public string? ImageUrl => ArtworkInfo.Thumbnails.PickClosest(300, 300)?.ImageUri.OriginalString;

    private static Task OpenAsync(Control? control, IArtworkInfo artworkInfo)
    {
        if (control?.GetViewContainer() is not { } viewContainer)
            return Task.CompletedTask;

        var viewModel = IllustrationItemViewModel.CreateInstance(artworkInfo);
        viewContainer.CreateIllustrationPage(viewModel, [viewModel]);
        return Task.CompletedTask;
    }
}

public sealed class HomeCardNovelPreviewItem(Novel novel) : HomeCardPreviewItem(control => OpenAsync(control, novel))
{
    public Novel Novel { get; } = novel;

    public string Title => Novel.Title;

    public string? ImageUrl => Novel.ThumbnailUrls.Medium;

    private static Task OpenAsync(Control? control, Novel novel)
    {
        if (control?.GetViewContainer() is { } viewContainer)
        {
            var viewModel = NovelItemViewModel.CreateInstance(novel);
            viewContainer.CreateNovelPage(viewModel, [viewModel]);
        }

        return Task.CompletedTask;
    }
}

public sealed class HomeCardUserPreviewItem(UserInfo user) : HomeCardPreviewItem(control => OpenAsync(control, user))
{
    public UserInfo User { get; } = user;

    public string AvatarUrl => User.ProfileImageUrls.Medium;

    private static async Task OpenAsync(Control? control, UserInfo user)
    {
        if (control?.GetViewContainer() is { } viewContainer)
            await viewContainer.CreateUserPageAsync(user.Id);
    }
}

public sealed class HomeCardSpotlightPreviewItem(Spotlight spotlight)
    : HomeCardPreviewItem(control => OpenAsync(control, spotlight))
{
    public Spotlight Spotlight { get; } = spotlight;

    public string Title => Spotlight.PureTitle;

    public string ImageUrl => Spotlight.Thumbnail;

    private static async Task OpenAsync(Control? control, Spotlight spotlight)
    {
        if (control?.GetTopLevel() is { Launcher: { } launcher })
            await launcher.LaunchUriAsync(new(spotlight.ArticleUrl));
    }
}

file static class HomePreviewControlExtensions
{
    extension(Control control)
    {
        public ViewContainerBase? GetViewContainer() => TopLevel.GetTopLevel(control)?.ViewContainer;

        public TopLevel? GetTopLevel() => TopLevel.GetTopLevel(control);
    }
}
