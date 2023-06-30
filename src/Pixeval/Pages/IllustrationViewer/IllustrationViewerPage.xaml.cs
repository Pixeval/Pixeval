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
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using Windows.Graphics;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Media;
using Pixeval.UserControls.IllustrationView;
using Pixeval.Util.Threading;
using Microsoft.UI.Windowing;
using Pixeval.Util.UI;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage : ISupportCustomTitleBarDragRegion
{
    // Tags for IllustrationInfoAndCommentsNavigationView

    private NavigationViewTag? _relatedWorksTag;

    private NavigationViewTag? _commentsTag;

    private NavigationViewTag? _illustrationInfoTag;

    private IllustrationViewerPageViewModel _viewModel = null!;

    private const double TitleBarHeight = 48;

    private readonly AsyncLatch<(CompositeTransform, double scale)> _zoomingAnimation;

    private readonly AsyncLatch _collapseThumbnailList;

    private static readonly EasingFunctionBase ExponentialEasingFunction = new ExponentialEase
    {
        EasingMode = EasingMode.EaseOut,
        Exponent = 12
    };

    public IllustrationViewerPage()
    {
        InitializeComponent();
        var dataTransferManager = UIHelper.GetDataTransferManager();
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;
        _zoomingAnimation = new(tuple =>
        {
            var (transform, scale) = tuple;
            var translateXAnimation = transform.CreateDoubleAnimation(
                nameof(CompositeTransform.ScaleX),
                new Duration(TimeSpan.FromMilliseconds(500)),
                ExponentialEasingFunction,
                from: transform.ScaleX,
                to: scale);

            var translateYAnimation = transform.CreateDoubleAnimation(
                nameof(CompositeTransform.ScaleY),
                new Duration(TimeSpan.FromMilliseconds(500)),
                ExponentialEasingFunction,
                from: transform.ScaleY,
                to: scale);
            UIHelper.CreateStoryboard(translateXAnimation, translateYAnimation).Begin();
            return Task.CompletedTask;
        });

        _collapseThumbnailList = new AsyncLatch(async () =>
        {
            await Task.Delay(3000);
            BottomCommandSection.Translation = new Vector3(0, 120, 0);
        });
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        SidePanelShadow.Receivers.Add(IllustrationPresenterDockPanel);
        CommandBorderDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);
        ThumbnailListDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);

        // IMPORTANT
        _viewModel.Snapshot = new(_viewModel.ContainerRiverFlowIllustrationViewViewModel!.DataProvider.IllustrationsSource);

        _collapseThumbnailList.RunAsync().Discard();
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new NavigatingFromIllustrationViewerMessage());
        foreach (var imageViewerPageViewModel in _viewModel.ImageViewerPageViewModels!)
        {
            imageViewerPageViewModel.ImageLoadingCancellationHandle.Cancel();
        }

        _viewModel.ZoomChanged -= OnZoomChanged;
        _viewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
        {
            animation.Configuration = new DirectConnectedAnimationConfiguration();
            animation.TryStart(IllustrationImageShowcaseFrame);
        }

        if (parameter is IllustrationViewerPageViewModel viewModel)
        {
            _viewModel = viewModel.IsDisposed ? viewModel.CreateNew() : viewModel;
            _viewModel.ZoomChanged += OnZoomChanged;
        }

        _relatedWorksTag = new NavigationViewTag(typeof(RelatedWorksPage), _viewModel);
        _illustrationInfoTag = new NavigationViewTag(typeof(IllustrationInfoPage), _viewModel);
        _commentsTag = new NavigationViewTag(typeof(CommentsPage), (App.AppViewModel.MakoClient.IllustrationComments(_viewModel.IllustrationId).Where(c => c is not null), _viewModel.IllustrationId));

        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.Current);

        // todo WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(_viewModel.IllustrationView?.GetItemContainer(_viewModel.IllustrationViewModelInTheGridView!) ?? App.AppViewModel.AppWindowRootFrame));
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.IsFullScreen = false;

    private void CurrentScalePercentage_OnLoaded(object sender, RoutedEventArgs e)
    {
        OnZoomChanged(null, 1); // trigger the first calculation of the percentage of the zooming, does not imply that there is a zooming happened.
    }

    private void TopCommandBarPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (_viewModel.IsFullScreen)
        {
            TopCommandBarAnimation.From = -TitleBarHeight;
            TopCommandBarAnimation.To = 0;
            TopCommandBarStoryboard.Begin();
        }
    }

    private void TopCommandBarPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (_viewModel.IsFullScreen)
        {
            TopCommandBarAnimation.From = 0;
            TopCommandBarAnimation.To = -TitleBarHeight;
            TopCommandBarStoryboard.Begin();
        }
    }

    private void IllustrationViewerPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CurrentScalePercentage.Text = $"{ScaledPercentage()}%";
    }

    private void RestoreResolutionToggleButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_viewModel.Current.Scale > 1)
        {
            _viewModel.Current.Zoom(1 - _viewModel.Current.Scale);
        }
        else if (IllustrationImageShowcaseFrame.FindDescendant<Image>() is { } imageControl)
        {
            var illustWidth = _viewModel.Current.IllustrationViewModel.Illustration.Width;
            var illustHeight = _viewModel.Current.IllustrationViewModel.Illustration.Height;
            var displayImageResolution = UIHelper.GetImageScaledFactor(
                illustWidth,
                illustHeight,
                imageControl.ActualWidth,
                imageControl.ActualHeight,
                _viewModel.Current.Scale);
            _viewModel.Current.Zoom(displayImageResolution >= 1 ? displayImageResolution * 2 : 1 / displayImageResolution - _viewModel.Current.Scale);
        }
    }

    private void OnZoomChanged(object? sender, double e)
    {
        if (IllustrationImageShowcaseFrame.FindDescendant<Image>() is { } imageControl)
        {
            if (imageControl.RenderTransform is CompositeTransform transform)
            {
                _zoomingAnimation.RunAsync((transform, e)).Discard();
            }

            CurrentScalePercentage.Text = $"{ScaledPercentage()}%";

            var scaled = Math.Abs(_viewModel.Current.Scale - 1) > double.Epsilon;
            RestoreResolutionToggleButton.IsChecked = scaled;
            _viewModel.FlipRestoreResolutionCommand(scaled);
        }
    }

    private int ScaledPercentage()
    {
        var illustWidth = _viewModel.Current.IllustrationViewModel.Illustration.Width;
        var illustHeight = _viewModel.Current.IllustrationViewModel.Illustration.Height;
        var imageControl = IllustrationImageShowcaseFrame.FindDescendant<Image>()!;
        var displayImageResolution = UIHelper.GetImageScaledFactor(
            illustWidth,
            illustHeight,
            imageControl.ActualWidth,
            imageControl.ActualHeight,
            _viewModel.Current.Scale);

        return (int)(displayImageResolution * 100);
    }

    private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
    {
        // all the illustrations in _viewModels only differ in different image sources
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
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.Next(),
            new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    private void PrevImage()
    {
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.Prev(),
            new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    private void NextIllustration()
    {
        var illustrationViewModel = (IllustrationViewModel)_viewModel.ContainerRiverFlowIllustrationViewViewModel!.DataProvider.IllustrationsView[_viewModel.IllustrationIndex!.Value + 1];
        var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

        NavigateSelf(new IllustrationViewerPageViewModel(_viewModel.IllustrationView!, viewModel),
            new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    private void PrevIllustration()
    {
        var illustrationViewModel = (IllustrationViewModel)_viewModel.ContainerRiverFlowIllustrationViewViewModel!.DataProvider.IllustrationsView[_viewModel.IllustrationIndex!.Value - 1];
        var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

        NavigateSelf(new IllustrationViewerPageViewModel(_viewModel.IllustrationView!, viewModel),
            new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
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

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        GenerateLinkToThisPageButtonTeachingTip.IsOpen = false;
        App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void IllustrationInfoAndCommentsNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
        {
            IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void IllustrationInfoAndCommentsSplitView_OnPaneOpenedOrClosed(SplitView sender, object args)
    {
        SetTitleBarDragRegion();
    }

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

    private void LeftPageButtonArea_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        PrevButtonDetector.Opacity = 1;
    }

    private void LeftPageButtonArea_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        PrevButtonDetector.Opacity = 0;
    }

    private void RightPageButtonArea_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        NextButtonDetector.Opacity = 1;
    }

    private void RightPageButtonArea_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        NextButtonDetector.Opacity = 0;
    }

    private void ThumbnailList_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.GetDataContext<IllustrationViewModel>();
        if (args.BringIntoViewDistanceX <= sender.ActualWidth)
        {
            _ = context.LoadThumbnailIfRequired();
        }

        if (sender is Border b && ThumbnailList.SelectedItem is not null)
        {
            b.BorderThickness = ThumbnailList.SelectedItem == context ? new Thickness(2) : new Thickness(0);
        }
    }

    private void ThumbnailList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView)
            return;

        const string borderControlName = "ThumbnailBorder";

        var item = listView.ContainerFromItem(listView.SelectedItem);
        if (item.FindDescendant(borderControlName) is Border border)
        {
            border.BorderThickness = new Thickness(2);
        }

        if (e.RemovedItems is [IllustrationViewModel removedItem]
            && listView.ContainerFromItem(removedItem) is { } container
            && container.FindDescendant(borderControlName) is Border removedBorder)
        {
            removedBorder.BorderThickness = new Thickness(0);
        }

        if (listView.SelectedItem is IllustrationViewModel viewModel && viewModel.Id != _viewModel.Current.IllustrationViewModel.Id)
        {
            var viewModels = viewModel.GetMangaIllustrationViewModels().ToArray();
            NavigateSelf(new IllustrationViewerPageViewModel(_viewModel.IllustrationView!, viewModels), new EntranceNavigationTransitionInfo());
        }
    }

    private void ThumbnailBorder_OnLoaded(object sender, RoutedEventArgs e)
    {
        var context = sender.GetDataContext<IllustrationViewModel>();
        if (context.Illustration.Id.ToString() == _viewModel.Current.IllustrationViewModel.Id && _viewModel.Snapshot is [..])
        {
            ThumbnailList.ScrollIntoView(context);
            if (_viewModel.Snapshot.IndexOf(context) is var index and not -1)
            {
                ThumbnailList.SelectedIndex = index;
            }
        }
    }

    private void SelectItemInListView(ListView listView, IllustrationViewModel newItem, IllustrationViewModel oldItem)
    {

        const string borderControlName = "ThumbnailBorder";
        listView.UpdateLayout();
        var item = listView.ContainerFromItem(listView.SelectedItem);
        if (item.FindDescendant(borderControlName) is Border border)
        {
            border.BorderThickness = new Thickness(2);
        }

        if (listView.ContainerFromItem(oldItem) is { } container
            && container.FindDescendant(borderControlName) is Border b)
        {
            b.BorderThickness = new Thickness(0);
        }
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
        var qrCodeSource = await UIHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustrationWebUri(_viewModel.Current.IllustrationViewModel.Id).ToString());
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

        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationAppUri(_viewModel.Current.IllustrationViewModel.Id).ToString()));
    }

    private void FullScreenTapped(object sender, TappedRoutedEventArgs e)
    {
        Window.AppWindow.SetPresenter(_viewModel.IsFullScreen ? AppWindowPresenterKind.Default : AppWindowPresenterKind.FullScreen);
        _viewModel.IsFullScreen = !_viewModel.IsFullScreen;
    }
}
