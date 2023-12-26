#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewerPage.xaml.cs
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
using Windows.Graphics;
using Windows.Storage.Streams;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage : SupportCustomTitleBarDragRegionPage
{
    private const ThumbnailUrlOption Option = ThumbnailUrlOption.SquareMedium;
    private bool _pointerNotInArea = true;

    private bool _timeUp;
    private IllustrationViewerPageViewModel _viewModel = null!;

    public IllustrationViewerPage() => InitializeComponent();

    public bool PointerNotInArea
    {
        get => _pointerNotInArea;
        set
        {
            _pointerNotInArea = value;
            if (Initialized && _pointerNotInArea && TimeUp)
                BottomCommandSection.Translation = new Vector3(0, 120, 0);
        }
    }

    public bool TimeUp
    {
        get => _timeUp;
        set
        {
            _timeUp = value;
            if (Initialized && _timeUp && PointerNotInArea)
                BottomCommandSection.Translation = new Vector3(0, 120, 0);
        }
    }

    protected override void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        var leftIndent = new RectInt32(
            0,
            0,
            _viewModel.IsInfoPaneOpen ? (int)IllustrationInfoAndCommentsSplitView.OpenPaneLength
                : 0,
            (int)TitleBarArea.ActualHeight);

        sender.SetRegionRects(NonClientRegionKind.Icon, [FromControl(Icon)]);
        sender.SetRegionRects(NonClientRegionKind.Passthrough, [GetScaledRect(leftIndent), FromControl(IllustrationViewerCommandBar), FromControl(IllustrationViewerSubCommandBar)]);
        titleBarHeight = 48;
    }

    /// <summary>
    /// <see cref="IllustrationViewerPage.OnPageDeactivated"/> might not be called when the window is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IllustrationViewerPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (parameter is not IllustrationViewerPageViewModel viewModel)
            return;
        _viewModel = viewModel;
        _viewModel.Window = Window;
        _viewModel.GenerateLinkTeachingTip = GenerateLinkTeachingTip;
        _viewModel.ShowQrCodeTeachingTip = ShowQrCodeTeachingTip;
        _viewModel.SnackBarTeachingTip = SnackBarTeachingTip;

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();
            if (args.PropertyName is not nameof(vm.CurrentIllustrationIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentIllustrationIndex
            var oldTag = args.OldTag.To<long>();
            var newTag = args.NewTag.To<long>(); // vm.CurrentPage.Id

            if (oldTag == newTag)
                return;
            var info = null as NavigationTransitionInfo;
            if (oldIndex < newIndex && oldIndex is not -1)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
            else if (oldIndex > newIndex)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };
            ThumbnailItemsView.StartBringItemIntoView(vm.CurrentIllustrationIndex, new BringIntoViewOptions { AnimationDesired = true });
            Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, vm.CurrentImage, info);
        };

        _viewModel.PropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();
            switch (args.PropertyName)
            {
                case nameof(IllustrationViewerPageViewModel.IsFullScreen):
                {
                    Window.AppWindow.SetPresenter(vm.IsFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
                    // 加载完之后设置标题栏
                    _ = Task.Delay(500).ContinueWith(_ => RaiseSetTitleBarDragRegion(), TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                }
                case IllustrationViewerPageViewModel.GenerateLink:
                {
                    vm.GenerateLinkTeachingTip.Target = GenerateLinkButton.IsInOverflow ? null : GenerateLinkButton;
                    break;
                }
                case IllustrationViewerPageViewModel.ShowQrCode:
                {
                    vm.ShowQrCodeTeachingTip.Target = ShowQrCodeButton.IsInOverflow ? null : ShowQrCodeButton;
                    break;
                }
                case IllustrationViewerPageViewModel.ShowShare:
                {
                    Window.ShowShareUi();
                    break;
                }
                case nameof(IllustrationViewerPageViewModel.IsInfoPaneOpen):
                {
                    if (vm.IsInfoPaneOpen)
                        IllustrationInfoAndCommentsNavigationViewNavigate(InfoPaneNavigationView, new SuppressNavigationTransitionInfo());
                    break;
                }
            }
        };

        // 第一次_viewModel.CurrentIllustrationIndex变化时，还没有订阅事件，所以不会触发DetailedPropertyChanged，需要手动触发
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.CurrentImage);
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var dataTransferManager = Window.GetDataTransferManager();
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;

        SidePanelShadow.Receivers.Add(IllustrationPresenterDockPanel);
        CommandBorderDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);
        ThumbnailListDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);

        IllustrationImageShowcaseFrame_OnTapped(null!, null!);
    }

    private Task IllustrationItemsView_OnLoadMoreRequested(object? sender, EventArgs e)
    {
        return _viewModel.LoadMoreAsync(20);
    }

    private async void IllustrationItemsView_OnElementPrepared(AdvancedItemsView sender, ItemContainer itemContainer)
    {
        var thumbnail = itemContainer.Child.To<IllustrationImage>();
        var viewModel = thumbnail.ViewModel;

        _ = await viewModel.TryLoadThumbnail(_viewModel, Option);
    }

    private void IllustrationItemsView_OnElementClearing(AdvancedItemsView sender, ItemContainer itemContainer)
    {
        var viewModel = itemContainer.Child.To<IllustrationImage>().ViewModel;

        viewModel.UnloadThumbnail(_viewModel, Option);
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.IsFullScreen = false;

    private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
    {
        // all the illustrations in _viewModels only differ in different image sources
        var vm = _viewModel.CurrentIllustration;
        if (_viewModel.CurrentImage.LoadSuccessfully)
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

        var thumbnailStream = await _viewModel.CurrentIllustration.GetThumbnail(Option);
        props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnailStream);
        request.Data.SetWebLink(webLink);

        if (vm.IsUgoira)
        {
            if (_viewModel.CurrentImage.OriginalImageSources is { } streams)
            {
                var metadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(vm.Id);
                var stream = await streams.UgoiraSaveToStreamAsync(metadata);
                var file = await AppKnownFolders.CreateTemporaryFileWithRandomNameAsync(IoHelper.GetUgoiraExtension());
                await stream.SaveToFileAsync(file);
                request.Data.SetStorageItems([file]);
            }
        }
        else
        {
            if (_viewModel.CurrentImage.OriginalImageSources?.FirstOrDefault() is { } stream)
            {
                var s = await stream.SaveToStreamAsync();
                var file = await AppKnownFolders.CreateTemporaryFileWithRandomNameAsync(".png");
                await s.SaveToFileAsync(file);
                request.Data.SetStorageItems([file]);
            }
        }
        // SetBitmap 无效
        // SetWebLink 后会导致 SetApplicationLink 无效
        // request.Data.SetApplicationLink(MakoHelper.GenerateIllustrationAppUri(vm.Id));

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
                // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
                ++ThumbnailItemsView.SelectedIndex;
                break;
            case null: break;
        }
    }

    private void NextButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        ++ThumbnailItemsView.SelectedIndex;
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
                // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
                --ThumbnailItemsView.SelectedIndex;
                break;
            case null: break;
        }
    }

    private void PrevButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        --ThumbnailItemsView.SelectedIndex;
    }

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        GenerateLinkTeachingTip.IsOpen = false;
        App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void IllustrationInfoAndCommentsNavigationViewOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        IllustrationInfoAndCommentsNavigationViewNavigate(sender, e.RecommendedNavigationTransitionInfo);
    }

    private void IllustrationInfoAndCommentsNavigationViewNavigate(NavigationView sender, NavigationTransitionInfo info)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
            _ = IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, info);
    }

    private void IllustrationInfoAndCommentsSplitView_OnPaneOpenedOrClosed(SplitView sender, object args) => RaiseSetTitleBarDragRegion();

    private async void IllustrationImageShowcaseFrame_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        BottomCommandSection.Translation = new Vector3();
        TimeUp = false;
        await Task.Delay(3000);
        TimeUp = true;
    }

    private void CommandBarElementOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var button = (ICommandBarElement)sender;
        ((FrameworkElement)sender).Width = button.IsInOverflow ? double.NaN : (double)Application.Current.Resources["CollapsedAppBarButtonWidth"];
    }

    private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
    {
        ThumbnailItemsView.TryLoadedFirst(sender.To<ItemContainer>());
    }
}
