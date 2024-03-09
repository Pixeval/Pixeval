#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationInfoPage.xaml.cs
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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using ReverseMarkdown;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationInfoPage
{
    private IllustrationInfoPageViewModel _viewModel = null!;

    public IllustrationInfoPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        _viewModel = new(e.Parameter.To<Illustration>());
        await SetIllustrationCaptionTextAsync();
        await _viewModel.LoadAvatarAsync();
    }

    private void IllustrationTagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new IllustrationTagClickedMessage((string)((Button)sender).Content));
    }

    private async void IllustratorPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(_viewModel.Illustrator.Id);
    }

    private void IllustrationInfoPage_OnUnloaded(object sender, RoutedEventArgs e) => _viewModel.Dispose();

    private async Task SetIllustrationCaptionTextAsync()
    {
        await Task.Yield();
        var caption = _viewModel.Illustration.Caption;
        string? md;
        if (string.IsNullOrEmpty(caption))
            md = IllustrationInfoPageResources.IllustrationCaptionEmpty;
        else
        {
            var markdownConverter = new Converter(new Config
            {
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                GithubFlavored = true
            });
            md = markdownConverter.Convert(caption);
        }
        IllustrationCaptionMarkdownTextBlock.Text = md.ReplaceLineEndings(Environment.NewLine + Environment.NewLine);
    }
}
