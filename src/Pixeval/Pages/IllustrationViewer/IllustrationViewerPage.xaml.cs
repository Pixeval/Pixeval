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
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage : SupportCustomTitleBarDragRegionPage
{
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
            if (IsLoaded && _pointerNotInArea && TimeUp)
                BottomCommandSection.Translation = new Vector3(0, 120, 0);
        }
    }

    public bool TimeUp
    {
        get => _timeUp;
        set
        {
            _timeUp = value;
            if (IsLoaded && _timeUp && PointerNotInArea)
                BottomCommandSection.Translation = new Vector3(0, 120, 0);
        }
    }

    protected override void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        if (_viewModel.IsFullScreen)
        {
            titleBarHeight = 0;
            return;
        }
        var leftIndent = new RectInt32(0, 0, IllustrationInfoAndCommentsSplitView.IsPaneOpen ? (int)IllustrationInfoAndCommentsSplitView.OpenPaneLength : 0, (int)TitleBarArea.ActualHeight);

        sender.SetRegionRects(NonClientRegionKind.Icon, [GetScaledRect(TitleBar.Icon)]);
        sender.SetRegionRects(NonClientRegionKind.Passthrough, [GetScaledRect(leftIndent), GetScaledRect(IllustrationViewerCommandBar), GetScaledRect(IllustrationViewerSubCommandBar)]);
        titleBarHeight = 48;
    }

    /// <summary>
    /// <see cref="EnhancedPage.OnPageDeactivated"/> might not be called when the window is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IllustrationViewerPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        // 此处this.XamlRoot为null
        _viewModel = Window.Content.To<FrameworkElement>().GetViewModel(parameter);

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();

            if (args.PropertyName is not nameof(vm.CurrentIllustrationIndex) and not nameof(vm.CurrentPageIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentIllustrationIndex

            var info = null as NavigationTransitionInfo;
            if (oldIndex < newIndex && oldIndex is not -1)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
            else if (oldIndex > newIndex)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };

            if (args.PropertyName is nameof(vm.CurrentIllustrationIndex))
            {
                var oldTag = args.OldTag.To<long>();
                var newTag = args.NewTag.To<long>(); // vm.CurrentPage.Id
                if (oldTag == newTag)
                    return;
                ThumbnailItemsView.StartBringItemIntoView(vm.CurrentIllustrationIndex, new BringIntoViewOptions { AnimationDesired = true });
            }

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

    private async void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
    {
        var viewModel = sender.GetDataContext<IllustrationItemViewModel>();
        _ = await viewModel.TryLoadThumbnailAsync(_viewModel);
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.FullScreenCommand.Execute(null);

    private async void OnDataTransferManagerOnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
    {
        var vm = _viewModel.CurrentIllustration;
        if (!_viewModel.CurrentImage.LoadSuccessfully)
            return;

        var request = args.Request;
        var deferral = request.GetDeferral();

        var props = request.Data.Properties;

        props.Title = EntryViewerPageResources.ShareTitleFormatted.Format(vm.Id);
        props.Description = vm.Title;

        var file = await _viewModel.CurrentImage.SaveToFolderAsync(AppKnownFolders.Temporary);
        request.Data.SetStorageItems([file]);
        request.Data.ShareCanceled += FileDispose;
        request.Data.ShareCompleted += FileDispose;
        deferral.Complete();
        return;

        async void FileDispose(DataPackage dataPackage, object o) => await file?.DeleteAsync(StorageDeleteOption.PermanentDelete);
    }

    private void NextButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        switch (_viewModel.NextButtonAction)
        {
            case true: ++PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: ++ThumbnailItemsView.SelectedIndex; break;
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
            case true: --PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: --ThumbnailItemsView.SelectedIndex; break;
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
        App.AppViewModel.AppSettings.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void IllustrationInfoAndCommentsNavigationViewOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
            _ = IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, e.RecommendedNavigationTransitionInfo);
    }

    private void IllustrationInfoAndCommentsSplitView_OnPaneOpenedOrClosed(SplitView sender, object args) => RaiseSetTitleBarDragRegion();

    private async void IllustrationImageShowcaseFrame_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        BottomCommandSection.Translation = new Vector3();
        TimeUp = false;
        await Task.Delay(3000);
        TimeUp = true;
    }

    private void Image_OnLoading(FrameworkElement sender, object args)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }
}
