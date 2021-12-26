#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationInfoPage.xaml.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Pages.Capability;
using Pixeval.Utilities;
using ReverseMarkdown;
using IllustratorPage = Pixeval.Pages.IllustratorViewer.IllustratorPage;

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
        _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
        SetIllustrationCaptionText();
    }

    private void IllustrationTagButton_OnClick(object sender, RoutedEventArgs e)
    {
        this.FindAscendant<IllustrationViewerPage>()?.GoBack();
        WeakReferenceMessenger.Default.Send(new IllustrationTagClickedMessage((string) ((Button) sender).Content));
    }

    private async void IllustrationCaptionMarkdownTextBlock_OnLinkClicked(object? sender, LinkClickedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri(e.Link));
    }

    private void IllustratorPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_viewModel.UserInfo is { } userInfo)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (UIElement)sender);
            App.AppViewModel.RootFrameNavigate(typeof(IllustratorPage), Tuple.Create((UIElement)sender, new IllustratorViewModel(userInfo)), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
        }
    }

    #region Helper Functions

    private IEnumerable<Tag> GetIllustrationTagItemSource()
    {
        return _viewModel.Current.IllustrationViewModel.Illustration.Tags ?? Enumerable.Empty<Tag>();
    }

    public static string GetMakoTagTranslatedNameText(string? name, string? fallback)
    {
        return (name.IsNullOrEmpty() ? fallback : name) ?? string.Empty;
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
        return _viewModel.Current.IllustrationViewModel.Illustration.Let(i => $"{i.Width} x {i.Height}") ?? IllustrationInfoPageResources.IllustrationDimensionUnknown;
    }

    private string GetIllustrationUploadDateText()
    {
        return _viewModel.Current.IllustrationViewModel.Illustration.CreateDate.ToString("yyyy-M-d HH:mm:ss");
    }

    private readonly Converter _markdownConverter = new(new Config
    {
        UnknownTags = Config.UnknownTagsOption.PassThrough,
        GithubFlavored = true
    });

    private void SetIllustrationCaptionText()
    {
        var caption = _viewModel.Current.IllustrationViewModel.Illustration.Caption;
        Task.Run(() => string.IsNullOrEmpty(caption) ? IllustrationInfoPageResources.IllustrationCaptionEmpty : _markdownConverter.Convert(caption))
            .ContinueWith(task => IllustrationCaptionMarkdownTextBlock.Text = task.Result, TaskScheduler.FromCurrentSynchronizationContext());
    }

    #endregion
}