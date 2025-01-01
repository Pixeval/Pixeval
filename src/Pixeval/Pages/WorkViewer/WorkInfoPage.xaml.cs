#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/WorkInfoPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using ReverseMarkdown;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class WorkInfoPage
{
    private WorkInfoPageViewModel<IWorkEntry> _viewModel = null!;

    public WorkInfoPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        _viewModel = new(e.Parameter.To<IWorkEntry>());
        await SetWorkCaptionTextAsync();
        await _viewModel.LoadAvatarAsync();
    }

    private void WorkTagButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(_viewModel.Entry is Illustration ? SimpleWorkType.IllustAndManga : SimpleWorkType.Novel, (string)((FrameworkElement)sender).Tag));
    }

    private async void IllustratorPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(_viewModel.Illustrator.Id);
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
