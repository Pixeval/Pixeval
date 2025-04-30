// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using ReverseMarkdown;
using WinUI3Utilities;
using System.Collections.Generic;
using Windows.System;

namespace Pixeval.Pages;

public sealed partial class WorkInfoPage
{
    private WorkInfoPageViewModel _viewModel = null!;

    public WorkInfoPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = new(e.Parameter.To<IArtworkInfo>());
        await SetWorkCaptionTextAsync();
    }

    private void WorkTagButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Entry is IWorkEntry entry)
            _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(
                entry is Illustration ? SimpleWorkType.IllustAndManga : SimpleWorkType.Novel,
                ((FrameworkElement) sender).GetTag<ITag>().Name));
    }

    private async void IllustratorPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        var user = sender.To<FrameworkElement>().GetTag<IUser>();
        if (user is UserEntity entity)
            await this.CreateIllustratorPageAsync(entity.Id);
        else
            await Launcher.LaunchUriAsync(user.WebsiteUri);
    }

    private async Task SetWorkCaptionTextAsync()
    {
        await Task.Yield();
        var caption = _viewModel.Entry.Description;
        if (string.IsNullOrEmpty(caption))
        {
            // md = WorkInfoPageResources.WorkCaptionEmpty;
            WorkCaptionMarkdownTextBlock.Visibility = Visibility.Collapsed;
            return;
        }
        var markdownConverter = new Converter(new Config
        {
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            GithubFlavored = true
        });
        var md = markdownConverter.Convert(caption);
        WorkCaptionMarkdownTextBlock.Text = md.ReplaceLineEndings(Environment.NewLine + Environment.NewLine);
        WorkCaptionMarkdownTextBlock.Visibility = Visibility.Visible;
    }

    private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        var tag = sender.To<FrameworkElement>().GetTag<Tag>();
        var blockedTags = App.AppViewModel.AppSettings.BlockedTags;
        if (!blockedTags.Contains(tag.Name))
        {
            _ = blockedTags.Add(tag.Name);
            AppInfo.SaveConfig(App.AppViewModel.AppSettings);
        }
    }

    private static string GetTagCategoryName(object o)
    {
        var grouping = (IGrouping<ITagCategory, ITag>) o;
        return grouping.Key.Name;
    }

    private static string GetTagCategoryTranslatedName(object o)
    {
        var grouping = (IGrouping<ITagCategory, ITag>) o;
        return grouping.Key.TranslatedName;
    }

    private static Visibility IsEmptyToVisibility(object o)
    {
        var grouping = (IGrouping<ITagCategory, ITag>) o;
        return grouping.Key == ITagCategory.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    public static string? PickClosestUri(IReadOnlyCollection<IImageFrame> frames, int width, int height)
        => frames.PickClosest(width, height)?.ImageUri.OriginalString;

    private Visibility GetSpacingVisibility(int x, int y)
        => x > 0 || y > 0 ? Visibility.Visible : Visibility.Collapsed;
}
