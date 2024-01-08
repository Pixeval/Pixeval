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

using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Utilities;
using ReverseMarkdown;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationInfoPage
{
    private IllustrationViewerPageViewModel _viewModel = null!;

    public IllustrationInfoPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _viewModel = e.Parameter.To<IllustrationViewerPageViewModel>();
        SetIllustrationCaptionText();
    }

    private void IllustrationTagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new IllustrationTagClickedMessage((string)((Button)sender).Content));
    }

    //private async void IllustrationCaptionMarkdownTextBlock_OnLinkClicked(object? sender, LinkClickedEventArgs e)
    //{
    //    await Launcher.LaunchUriAsync(new Uri(e.Link));
    //}

    private void IllustratorPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_viewModel.Illustrator is { } userInfo)
        {
            _ = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (UIElement)sender);
            // todo IllustratorPage use Navigate
            // CurrentContext.Window.Content.To<Frame>().Navigate(typeof(IllustratorPage), Tuple.Create((UIElement)sender, new IllustratorViewModel(userInfo)), new SlideNavigationTransitionInfo
            // {
            //     Effect = SlideNavigationTransitionEffect.FromRight
            // });
        }
    }

    #region Helper Functions

    private IEnumerable<Tag> GetIllustrationTagItemSource()
    {
        return _viewModel.CurrentIllustration.Illustrate.Tags;
    }

    public static string GetMakoTagTranslatedNameText(string? name, string fallback)
    {
        return name.IsNullOrEmpty() ? fallback : name;
    }

    private string GetIllustratorNameText()
    {
        return IllustrationInfoPageResources.IllustratorNameFormatted.Format(_viewModel.IllustratorName);
    }

    private string GetIllustratorIdText()
    {
        return IllustrationInfoPageResources.IllustratorIdFormatted.Format(_viewModel.IllustratorUid);
    }

    private string GetIllustrationDimensionText()
    {
        return _viewModel.CurrentIllustration.Illustrate.Let(i => $"{i.Width} x {i.Height}") ?? IllustrationInfoPageResources.IllustrationDimensionUnknown;
    }

    private string GetIllustrationUploadDateText()
    {
        return _viewModel.CurrentIllustration.Illustrate.CreateDate.ToString("yyyy-M-d HH:mm:ss");
    }

    private readonly Converter _markdownConverter = new(new Config
    {
        UnknownTags = Config.UnknownTagsOption.PassThrough,
        GithubFlavored = true
    });

    /// <summary>
    /// <see href="https://github.com/CommunityToolkit/Labs-Windows/pull/480"/>
    /// </summary>
    private void SetIllustrationCaptionText()
    {
        var caption = _viewModel.CurrentIllustration.Illustrate.Caption;
        // TODO Markdown Task.Run(() => string.IsNullOrEmpty(caption) ? IllustrationInfoPageResources.IllustrationCaptionEmpty : _markdownConverter.Convert(caption))
        //    .ContinueWith(task => IllustrationCaptionMarkdownTextBlock.Text = task.Result, TaskScheduler.FromCurrentSynchronizationContext()).Discard();
    }

    #endregion
}
