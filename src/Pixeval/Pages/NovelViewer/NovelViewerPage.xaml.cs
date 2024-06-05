using System.Numerics;
using System.Threading.Tasks;
using Windows.System;
using WinUI3Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using WinRT;
using Pixeval.Controls.Windowing;

namespace Pixeval.Pages.NovelViewer;

public sealed partial class NovelViewerPage
{
    private bool _pointerNotInArea = true;

    private bool _timeUp;

    private NovelViewerPageViewModel _viewModel = null!;

    public NovelViewerPage() => InitializeComponent();

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
        _viewModel = HWnd.GetViewModel(parameter);

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<NovelViewerPageViewModel>();

            if (args.PropertyName is not nameof(vm.CurrentNovelIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentNovelIndex

            EntryViewerSplitView.NavigationViewSelect(vm.Tags[0]);
            ThumbnailItemsView.StartBringItemIntoView(vm.CurrentNovelIndex, new BringIntoViewOptions { AnimationDesired = true });
        };


        _viewModel.PropertyChanged += (sender, args) =>
        {
            var vm = sender.To<NovelViewerPageViewModel>();
            switch (args.PropertyName)
            {
                case nameof(NovelViewerPageViewModel.IsFullScreen):
                {
                    var window = WindowFactory.ForkedWindows[HWnd];
                    window.AppWindow.SetPresenter(vm.IsFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
                    break;
                }
            }
        }; 
    }

    private void NovelViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        CommandBorderDropShadow.Receivers.Add(DocumentViewer);
        ThumbnailListDropShadow.Receivers.Add(DocumentViewer);

        ThumbnailItemsView.StartBringItemIntoView(_viewModel.CurrentNovelIndex, new BringIntoViewOptions { AnimationDesired = true });
        DocumentViewer_OnTapped(null!, null!);
    }

    private void Panel_OnLoaded(object sender, RoutedEventArgs e)
    {
        var panel = sender.To<StackPanel>();

        foreach (var entry in _viewModel.Entries)
            panel.Children.Add(entry.Element);
    }

    private void NovelViewerPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }

    private async void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        var viewModel = sender.GetDataContext<NovelItemViewModel>();
        _ = await viewModel.TryLoadThumbnailAsync(_viewModel);
    }

    private void ExitFullScreenKeyboardAccelerator_OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => _viewModel.IsFullScreen = false;

    private void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        _viewModel.CurrentNovel.AddToBookmarkCommand.Execute((BookmarkTagSelector.SelectedTags, BookmarkTagSelector.IsPrivate, DownloadParameter(DocumentViewer.ViewModel)));
    }

    private void AddToBookmarkButton_OnClicked(object sender, RoutedEventArgs e) => AddToBookmarkTeachingTip.IsOpen = true;

    private void NextButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.NextButtonAction)
        {
            case true: ++PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentNovelIndex
            case false: ++ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void NextButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentNovelIndex
        ++ThumbnailItemsView.SelectedIndex;
    }

    private void PrevButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.PrevButtonAction)
        {
            case true: --PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentNovelIndex
            case false: --ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void PrevButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentNovelIndex
        --ThumbnailItemsView.SelectedIndex;
    }

    private void ThumbnailItemsView_OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        switch (e.Key)
        {
            case VirtualKey.Left:
                PrevButton_OnClicked(null!, null!);
                break;
            case VirtualKey.Right:
                NextButton_OnClicked(null!, null!);
                break;
            case VirtualKey.Up:
                PrevButton_OnRightTapped(null!, null!);
                break;
            case VirtualKey.Down:
                NextButton_OnRightTapped(null!, null!);
                break;
        }
    }

    private async void DocumentViewer_OnTapped(object sender, TappedRoutedEventArgs e)
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

    private void OpenPane_OnRightTapped(object sender, RightTappedRoutedEventArgs e) => EntryViewerSplitView.PinPane = true;

    public (ulong, DocumentViewerViewModel?) DownloadParameter(DocumentViewerViewModel? viewModel) => (HWnd, viewModel);
}
