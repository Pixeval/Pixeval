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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinRT;
using WinUI3Utilities;
using Windows.System;
using Pixeval.Controls.Windowing;
using Pixeval.Upscaling;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage
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

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        // 此处this.XamlRoot为null
        _viewModel = HWnd.GetIllustrationViewerPageViewModelFromHandle(parameter);

        _viewModel.CurrentImage.UpscalerMessageChannel.Reader.OnReceive(
            reader => reader == _viewModel.CurrentImage.UpscalerMessageChannel.Reader,
            OnReceiveUpscalerMessage);

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();

            if (args.PropertyName is not nameof(vm.CurrentIllustrationIndex) and not nameof(vm.CurrentPageIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentIllustrationIndex

            NavigationTransitionInfo? info = null;
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
                EntryViewerSplitView.NavigationViewSelect(vm.Tags[0]);
            }

            Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, vm.CurrentImage, info);

            vm.CurrentImage.UpscalerMessageChannel.Reader.OnReceive(
                reader => reader == vm.CurrentImage.UpscalerMessageChannel.Reader,
                OnReceiveUpscalerMessage);
        };

        _viewModel.PropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();
            switch (args.PropertyName)
            {
                case nameof(IllustrationViewerPageViewModel.IsFullScreen):
                {
                    var window = WindowFactory.ForkedWindows[HWnd];
                    window.AppWindow.SetPresenter(vm.IsFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
                    break;
                }
            }
        };

        // 第一次_viewModel.CurrentIllustrationIndex变化时，还没有订阅事件，所以不会触发DetailedPropertyChanged，需要手动触发
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.CurrentImage);
    }

    [GeneratedRegex(@"\d+\.\d+%")]
    private static partial Regex UpscalerMessagePercentageRegex();

    private void OnReceiveUpscalerMessage(string message)
    {
        _viewModel.UpscalerProgressBarVisible = UpscalerMessagePercentageRegex().IsMatch(message);
        _viewModel.AdditionalTextBlockVisible = !_viewModel.UpscalerProgressBarVisible;
        if (message == Upscaler.ProcessCompletedMark)
        {
            _viewModel.UpscalerProgressText = string.Empty;
            _viewModel.UpscalerProgress = 0;
            _viewModel.AdditionalText = $"{EntryViewerPageResources.AiUpscaled}";
            return;
        }

        if (UpscalerMessagePercentageRegex().IsMatch(message))
        {
            _viewModel.UpscalerProgressText = message;
            _viewModel.UpscalerProgress = (int) double.Parse(message[..^1]);
            return;
        }

        _viewModel.AdditionalText = message;
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!App.AppViewModel.AppSettings.BrowseOriginalImage)
        {
            _viewModel.AdditionalText = EntryViewerPageResources.BrowsingCompressedImage;
        }

        // Invokes the drag region calculation manually 9/11/2024
        TitleBarArea.SetDragRegionForCustomTitleBar();
        var dataTransferManager = HWnd.GetDataTransferManager();
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;

        CommandBorderDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);
        ThumbnailListDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);

        ThumbnailItemsView.StartBringItemIntoView(_viewModel.CurrentIllustrationIndex, new BringIntoViewOptions { AnimationDesired = true });
        IllustrationImageShowcaseFrame_OnTapped(null!, null!);
    }

    private void IllustrationViewerPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }

    private async void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
    {
        var viewModel = sender.GetDataContext<IllustrationItemViewModel>();
        _ = await viewModel.TryLoadThumbnailAsync(_viewModel);
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.IsFullScreen = false;

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

    private void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        _viewModel.CurrentIllustration.AddToBookmarkCommand.Execute((BookmarkTagSelector.SelectedTags, BookmarkTagSelector.IsPrivate, _viewModel.CurrentImage.DownloadParameter));

        HWnd.SuccessGrowl(EntryViewerPageResources.AddedToBookmark);
    }

    private void AddToBookmarkButton_OnClicked(object sender, RoutedEventArgs e) => AddToBookmarkTeachingTip.IsOpen = true;

    private void NextButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.NextButtonAction)
        {
            case true: ++PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: ++ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void NextButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        ++ThumbnailItemsView.SelectedIndex;
    }

    private void PrevButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.PrevButtonAction)
        {
            case true: --PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: --ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void PrevButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        --ThumbnailItemsView.SelectedIndex;
    }

    private void ThumbnailItemsView_OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        switch (e.Key)
        {
            case VirtualKey.Left: PrevButton_OnClicked(null!, null!); break;
            case VirtualKey.Right: NextButton_OnClicked(null!, null!); break;
            case VirtualKey.Up: PrevButton_OnRightTapped(null!, null!); break;
            case VirtualKey.Down: NextButton_OnRightTapped(null!, null!); break;
        }
    }

    private async void IllustrationImageShowcaseFrame_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        BottomCommandSection.Translation = new Vector3();
        TimeUp = false;
        await Task.Delay(3000);
        TimeUp = true;
    }

    private void Content_OnLoading(FrameworkElement sender, object args)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }

    private async void UpscaleButton_OnTapped(object sender, RoutedEventArgs e)
    {
        if (!App.AppViewModel.AppSettings.ShowUpscalerTeachingTip)
        {
            _viewModel.CurrentImage.UpscaleCommand.Execute(null);
            return;
        }
        UpscaleTeachingTip.IsOpen = true;
        var dialog = await HWnd.CreateOkCancelAsync(EntryViewerPageResources.AiUpscalerWarningTitle,
            EntryViewerPageResources.AiUpscalerWarningContent,
            EntryViewerPageResources.AiUpscalerWarningOkButtonContent,
            EntryViewerPageResources.AiUpscalerWarningCancelButtonContent);

        if (dialog == ContentDialogResult.Primary)
        {
            _viewModel.CurrentImage.UpscaleCommand.Execute(null);
        }

        if (App.AppViewModel.AppSettings.ShowUpscalerTeachingTip)
        {
            App.AppViewModel.AppSettings.ShowUpscalerTeachingTip = false;
        }
    }

    public Visibility IsLogoVisible()
    {
        return WindowFactory.GetWindowForElement(this).HWnd != WindowFactory.RootWindow.HWnd
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
