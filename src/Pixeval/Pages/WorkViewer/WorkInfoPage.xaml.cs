// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using ReverseMarkdown;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class WorkInfoPage
{
    private WorkInfoPageViewModel<IWorkEntry> _viewModel = null!;

    public WorkInfoPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = new(e.Parameter.To<IWorkEntry>());
        await SetWorkCaptionTextAsync();
        await _viewModel.LoadAvatarAsync();
    }

    private void WorkTagButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(_viewModel.Entry is Illustration ? SimpleWorkType.IllustAndManga : SimpleWorkType.Novel, (string) ((FrameworkElement) sender).Tag));
    }

    private async void IllustratorPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(_viewModel.Illustrator.Id);
    }

    private async Task SetWorkCaptionTextAsync()
    {
        await Task.Yield();
        var caption = _viewModel.Entry.Caption;
        string? md;
        if (string.IsNullOrEmpty(caption))
            md = WorkInfoPageResources.WorkCaptionEmpty;
        else
        {
            var markdownConverter = new Converter(new Config
            {
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                GithubFlavored = true
            });
            md = markdownConverter.Convert(caption);
        }
        WorkCaptionMarkdownTextBlock.Text = md.ReplaceLineEndings(Environment.NewLine + Environment.NewLine);
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
}
