﻿#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationViewerPage.xaml.cs
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
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Popups;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage : IGoBack
{
    private AppPopup? _commentRepliesPopup;

    private NavigationViewTag? _comments;

    // Tags for IllustrationInfoAndCommentsNavigationView

    private NavigationViewTag? _illustrationInfo;

    private IllustrationViewerPageViewModel _viewModel = null!;

    public IllustrationViewerPage()
    {
        InitializeComponent();
        var dataTransferManager = UIHelper.GetDataTransferManager();
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;
    }

    public void GoBack()
    {
        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", IllustrationImageShowcaseFrame);
        WeakReferenceMessenger.Default.Send(new NavigatingBackToMainPageMessage(_viewModel.IllustrationViewModelInTheGridView));

        App.AppViewModel.AppWindowRootFrame.BackStack.RemoveAll(entry => entry.SourcePageType == typeof(IllustrationViewerPage));
        if (App.AppViewModel.AppWindowRootFrame.CanGoBack)
        {
            App.AppViewModel.AppWindowRootFrame.GoBack(new SuppressNavigationTransitionInfo());
        }
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        SidePanelShadow.Receivers.Add(IllustrationPresenterDockPanel);
        PopupShadow.Receivers.Add(IllustrationInfoAndCommentsSplitView);
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        foreach (var imageViewerPageViewModel in _viewModel.ImageViewerPageViewModels!)
        {
            imageViewerPageViewModel.ImageLoadingCancellationHandle.Cancel();
        }

        _viewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
        {
            animation.Configuration = new DirectConnectedAnimationConfiguration();
            animation.TryStart(IllustrationImageShowcaseFrame);
        }

        _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
        _illustrationInfo = new NavigationViewTag(typeof(IllustrationInfoPage), _viewModel);
        _comments = new NavigationViewTag(typeof(CommentsPage), (App.AppViewModel.MakoClient.IllustrationComments(_viewModel.IllustrationId).Where(c => c is not null), _viewModel.IllustrationId)); // TODO

        IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Current);

        WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(_viewModel.IllustrationGrid?.GetItemContainer(_viewModel.IllustrationViewModelInTheGridView!) ?? App.AppViewModel.AppWindowRootFrame));
        WeakReferenceMessenger.Default.Register<IllustrationViewerPage, CommentRepliesHyperlinkButtonTappedMessage>(this, CommentRepliesHyperlinkButtonTapped);
    }

    private static void CommentRepliesHyperlinkButtonTapped(IllustrationViewerPage recipient, CommentRepliesHyperlinkButtonTappedMessage message)
    {
        var commentRepliesBlock = new CommentRepliesBlock
        {
            ViewModel = new CommentRepliesBlockViewModel(message.Sender!.GetDataContext<CommentBlockViewModel>())
        };
        commentRepliesBlock.CloseButtonTapped += recipient.CommentRepliesBlock_OnCloseButtonTapped;
        recipient._commentRepliesPopup = PopupManager.CreatePopup(commentRepliesBlock, widthMargin: 200, maxWidth: 1500, minWidth: 400, maxHeight: 1200, closing: (_, _) => recipient._commentRepliesPopup = null);
        PopupManager.ShowPopup(recipient._commentRepliesPopup);
    }

    private void CommentRepliesBlock_OnCloseButtonTapped(object? sender, TappedRoutedEventArgs e)
    {
        if (_commentRepliesPopup is not null)
        {
            PopupManager.ClosePopup(_commentRepliesPopup);
        }
    }

    private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
    {
        // Remarks: all the illustrations in _viewModels only differ in different image sources
        var vm = _viewModel.Current.IllustrationViewModel;
        if (_viewModel.Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
        {
            return;
        }

        var request = args.Request;
        var deferral = request.GetDeferral();
        var props = request.Data.Properties;
        var webLink = MakoHelper.GenerateIllustrationWebUri(vm.Id);

        props.Title = IllustrationViewerPageResources.ShareTitleFormatted.Format(vm.Id);
        props.Description = vm.Illustration.Title;
        props.Square30x30Logo = RandomAccessStreamReference.CreateFromStream(await AppContext.GetAssetStreamAsync("Images/logo44x44.ico"));

        var thumbnailStream = await _viewModel.Current.IllustrationViewModel.GetThumbnail(ThumbnailUrlOption.SquareMedium);
        var file = await AppKnownFolders.CreateTemporaryFileWithRandomNameAsync(_viewModel.IsUgoira ? "gif" : "png");

        if (_viewModel.Current.OriginalImageStream is { } stream)
        {
            await stream.SaveToFileAsync(file);

            props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnailStream);

            request.Data.SetStorageItems(Enumerates.ArrayOf(file), true);
            request.Data.SetWebLink(webLink);
            request.Data.SetApplicationLink(MakoHelper.GenerateIllustrationAppUri(vm.Id));
        }

        deferral.Complete();
    }

    private void NextImage()
    {
        IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Next(), new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromRight
        });
    }

    private void PrevImage()
    {
        IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Prev(), new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromLeft
        });
    }

    private void NextIllustration()
    {
        var illustrationViewModel = (IllustrationViewModel) _viewModel.ContainerGridViewModel!.IllustrationsView[_viewModel.IllustrationIndex!.Value + 1];
        var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

        App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(_viewModel.IllustrationGrid!, viewModel), new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromRight
        });
    }

    private void PrevIllustration()
    {
        var illustrationViewModel = (IllustrationViewModel) _viewModel.ContainerGridViewModel!.IllustrationsView[_viewModel.IllustrationIndex!.Value - 1];
        var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

        App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(_viewModel.IllustrationGrid!, viewModel), new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromLeft
        });
    }

    private void NextImageAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        NextImage();
    }

    private void PrevImageAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        PrevImage();
    }

    private void NextIllustrationAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        NextIllustration();
    }

    private void PrevIllustrationAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        PrevIllustration();
    }

    private void BackButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        GoBack();
    }

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        _viewModel.IsGenerateLinkTeachingTipOpen = false;
        App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void IllustrationInfoAndCommentsNavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        IllustrationInfoAndCommentsSplitView.IsPaneOpen = false;
    }

    private void IllustrationInfoAndCommentsNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
        {
            IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, new SlideNavigationTransitionInfo
            {
                Effect = tag switch
                {
                    var x when x == _illustrationInfo => SlideNavigationTransitionEffect.FromLeft,
                    var x when x == _comments => SlideNavigationTransitionEffect.FromRight,
                    _ => throw new ArgumentOutOfRangeException()
                }
            });
        }
    }
}