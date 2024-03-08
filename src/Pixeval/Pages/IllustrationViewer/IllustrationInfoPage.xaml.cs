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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Utilities;
using ReverseMarkdown;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationInfoPage
{
    private IllustrationViewerPageViewModel _viewModel = null!;

    public IllustrationInfoPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _viewModel = e.Parameter.To<IllustrationViewerPageViewModel>();
        SetIllustrationCaptionText();
    }

    private void IllustrationTagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new IllustrationTagClickedMessage((string)((Button)sender).Content));
    }

    private async void IllustratorPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(_viewModel.IllustratorId);
    }

    #region Helper Functions

    private string GetIllustratorNameText()
    {
        return IllustrationInfoPageResources.IllustratorNameFormatted.Format(_viewModel.IllustratorName);
    }

    private string GetIllustratorIdText()
    {
        return IllustrationInfoPageResources.IllustratorIdFormatted.Format(_viewModel.IllustratorId);
    }

    private string GetIllustrationDimensionText()
    {
        return _viewModel.CurrentIllustration.Entry.Let(i => $"{i.Width} x {i.Height}") ?? IllustrationInfoPageResources.IllustrationDimensionUnknown;
    }

    private async void SetIllustrationCaptionText()
    {
        await Task.Yield();
        var markdownConverter = new Converter(new Config
        {
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            GithubFlavored = true
        });
        var caption = _viewModel.CurrentIllustration.Entry.Caption;
        var md = string.IsNullOrEmpty(caption)
            ? IllustrationInfoPageResources.IllustrationCaptionEmpty
            : markdownConverter.Convert(caption);
        IllustrationCaptionMarkdownTextBlock.Text = md.ReplaceLineEndings(Environment.NewLine + Environment.NewLine);
    }

    #endregion
}
