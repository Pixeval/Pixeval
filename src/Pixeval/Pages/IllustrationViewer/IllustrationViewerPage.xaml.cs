#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationViewerPage.xaml.cs
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
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.UserControls.IllustrationView;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage.Streams;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage : ISupportCustomTitleBarDragRegion
{
    // Tags for IllustrationInfoAndCommentsNavigationView

    private NavigationViewTag? _relatedWorksTag;

    private NavigationViewTag? _commentsTag;

    private NavigationViewTag? _illustrationInfoTag;

    private IllustrationViewerPageViewModel _viewModel = null!;

    private const double TitleBarHeight = 48;
    private const double NegativeTitleBarHeight = -TitleBarHeight;

    private readonly AsyncLatch _collapseThumbnailList;

    public IllustrationViewerPage()
    {
        InitializeComponent();

        _collapseThumbnailList = new AsyncLatch(async () =>
        {
            _viewModel.TimeUp = false;
            await Task.Delay(3000);
            _viewModel.TimeUp = true;
        });
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        _viewModel.Dispose();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (parameter is IllustrationViewerPageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        _relatedWorksTag = new NavigationViewTag(typeof(RelatedWorksPage), _viewModel);
        _illustrationInfoTag = new NavigationViewTag(typeof(IllustrationInfoPage), _viewModel);
        _commentsTag = new NavigationViewTag(typeof(CommentsPage), (App.AppViewModel.MakoClient.IllustrationComments(_viewModel.IllustrationId).Where(c => c is not null), _viewModel.IllustrationId));

        ThumbnailList.SelectedIndex = _viewModel.CurrentIllustrationIndex;
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var dataTransferManager = Window.GetDataTransferManager();
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;

        SidePanelShadow.Receivers.Add(IllustrationPresenterDockPanel);
        CommandBorderDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);
        ThumbnailListDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);

        _viewModel.CollapseThumbnailList = () => BottomCommandSection.Translation = new Vector3(0, 120, 0);
        _collapseThumbnailList.RunAsync().Discard();
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.IsFullScreen = false;

    private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
    {
        // all the illustrations in _viewModels only differ in different image sources
        var vm = _viewModel.CurrentIllustration;
        if (_viewModel.CurrentViewModel.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
        {
            return;
        }

        var request = args.Request;
        var deferral = request.GetDeferral();
        var props = request.Data.Properties;
        var webLink = MakoHelper.GenerateIllustrationWebUri(vm.Id);

        props.Title = IllustrationViewerPageResources.ShareTitleFormatted.Format(vm.Id);
        props.Description = vm.Illustrate.Title;
        props.Square30x30Logo = RandomAccessStreamReference.CreateFromStream(await AppContext.GetAssetStreamAsync("Images/logo44x44.ico"));

        var thumbnailStream = await _viewModel.CurrentIllustration.GetThumbnail(ThumbnailUrlOption.SquareMedium);
        props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnailStream);
        request.Data.SetWebLink(webLink);

        if (_viewModel.CurrentViewModel.OriginalImageStream is { } stream)
        {
            var file = await AppKnownFolders.CreateTemporaryFileWithRandomNameAsync(_viewModel.IsUgoira ? "gif" : "png");
            await stream.SaveToFileAsync(file);
            request.Data.SetStorageItems(Enumerates.ArrayOf(file), true);
            // SetBitmap 无效
            // SetWebLink 后会导致 SetApplicationLink 无效
            // request.Data.SetApplicationLink(MakoHelper.GenerateIllustrationAppUri(vm.Id));
        }

        deferral.Complete();
    }

    private void NextButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        switch (_viewModel.NextButtonAction)
        {
            case true:
                Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.NextPage(),
                    new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
                break;
            case false:
                ++ThumbnailList.SelectedIndex;
                break;
            case null: break;
        }
    }

    private void NextButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        ++ThumbnailList.SelectedIndex;
    }

    private void PrevButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        switch (_viewModel.PrevButtonAction)
        {
            case true:
                Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.PrevPage(),
                    new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
                break;
            case false:
                --ThumbnailList.SelectedIndex;
                break;
            case null: break;
        }
    }

    private void PrevButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        --ThumbnailList.SelectedIndex;
    }

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        GenerateLinkToThisPageButtonTeachingTip.IsOpen = false;
        App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void IllustrationInfoAndCommentsNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
        {
            _ = IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void IllustrationInfoAndCommentsSplitView_OnPaneOpenedOrClosed(SplitView sender, object args) => SetTitleBarDragRegion();

    public void SetTitleBarDragRegion()
    {
        var pointCommandBar = IllustrationViewerCommandBar.TransformToVisual(this).TransformPoint(new Point(0, 0));
        var pointSubCommandBar = IllustrationViewerSubCommandBar.TransformToVisual(this).TransformPoint(new Point(0, 0));
        var commandBarRect = new RectInt32((int)pointCommandBar.X, (int)pointCommandBar.Y, (int)IllustrationViewerCommandBar.ActualWidth, (int)IllustrationViewerCommandBar.ActualHeight);
        var subCommandBarRect = new RectInt32((int)pointSubCommandBar.X, (int)pointSubCommandBar.Y, (int)IllustrationViewerSubCommandBar.ActualWidth, (int)IllustrationViewerSubCommandBar.ActualHeight);

        Window.SetDragRegion(new(commandBarRect, subCommandBarRect)
        {
            DragZoneLeftIndent = _viewModel.IsInfoPaneOpen
                ? (int)IllustrationInfoAndCommentsSplitView.OpenPaneLength
                : 0
        });
    }

    private void ThumbnailOnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.GetDataContext<IllustrationViewModel>();
        if (args.BringIntoViewDistanceX <= sender.ActualWidth)
        {
            _ = context.LoadThumbnailIfRequired();
        }
    }

    private void ThumbnailList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListViewBase list)
            return;

        var oldIndex = -1;
        if (e.RemovedItems is [IllustrationViewModel removedItem])
        {
            oldIndex = Array.IndexOf(_viewModel.Illustrations, removedItem);
        }

        if (list.SelectedItem is not IllustrationViewModel viewModel)
            return;

        var info = (NavigationTransitionInfo?)null;
        if (oldIndex < list.SelectedIndex && oldIndex is not -1)
            info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
        else if (oldIndex > list.SelectedIndex)
            info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };
        list.ScrollIntoView(list.SelectedItem);
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.Goto(viewModel), info);
    }

    private void IllustrationImageShowcaseFrame_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        BottomCommandSection.Translation = new Vector3();
        _collapseThumbnailList.RunAsync().Discard();
    }

    private void CommandBarElementOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var button = (ICommandBarElement)sender;
        ((FrameworkElement)sender).Width = button.IsInOverflow ? double.NaN : (double)Application.Current.Resources["CollapsedAppBarButtonWidth"];
    }

    private async void ShowQrCodeOnTapped(object sender, TappedRoutedEventArgs e)
    {
        var button = sender.To<AppBarButton>();
        var qrCodeSource = await UIHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustrationWebUri(_viewModel.CurrentIllustration.Id).ToString());
        QrCodeTeachingTip.HeroContent.To<Image>().Source = qrCodeSource;
        QrCodeTeachingTip.Target = button.IsInOverflow ? null : button;
        QrCodeTeachingTip.IsOpen = true;

        void Closed(TeachingTip s, TeachingTipClosedEventArgs ea)
        {
            qrCodeSource.Dispose();
            s.Closed -= Closed;
        }

        QrCodeTeachingTip.Closed += Closed;
    }

    private void GenerateLinkCommandOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
        {
            var button = (AppBarButton)sender;
            GenerateLinkToThisPageButtonTeachingTip.Target = button.IsInOverflow ? null : button;
            GenerateLinkToThisPageButtonTeachingTip.IsOpen = true;
        }

        UIHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(_viewModel.CurrentIllustration.Id).ToString());
    }

    private void ShareOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_viewModel.CurrentViewModel.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
        {
            _viewModel.TeachingTipProperties.ShowAndHide(IllustrationViewerPageResources.CannotShareImageForNowTitle, TeachingTipSeverity.Warning,
                IllustrationViewerPageResources.CannotShareImageForNowContent);
            return;
        }

        Window.ShowShareUI();
    }

    private void FullScreenTapped(object sender, TappedRoutedEventArgs e)
    {
        Window.AppWindow.SetPresenter(_viewModel.IsFullScreen ? AppWindowPresenterKind.Default : AppWindowPresenterKind.FullScreen);
        _viewModel.IsFullScreen = !_viewModel.IsFullScreen;
    }

    private void ThumbnailListGrid_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _viewModel.PointerNotInArea = false;
    }

    private void ThumbnailListGrid_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _viewModel.PointerNotInArea = true;
    }
}
