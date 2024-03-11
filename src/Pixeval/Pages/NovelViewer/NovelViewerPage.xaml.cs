using System.Threading.Tasks;
using Windows.Graphics;
using WinUI3Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Pages.NovelViewer;

public sealed partial class NovelViewerPage
{
    private NovelViewerPageViewModel _viewModel = null!;

    public NovelViewerPage() => InitializeComponent();

    protected override void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        var leftIndent = new RectInt32(0, 0, EntryViewerSplitView.IsPaneOpen ? (int)WorkViewerSplitView.OpenPaneLength : 0, (int)TitleBarArea.ActualHeight);

        if (TitleBar.Visibility is Visibility.Visible)
        {
            sender.SetRegionRects(NonClientRegionKind.Icon, [GetScaledRect(TitleBar.Icon)]);
        }
        sender.SetRegionRects(NonClientRegionKind.Passthrough, [GetScaledRect(leftIndent), GetScaledRect(NovelViewerCommandBar)]);
        titleBarHeight = 48;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        // 此处this.XamlRoot为null
        _viewModel = Window.Content.To<FrameworkElement>().GetViewModel(parameter);

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<NovelViewerPageViewModel>();

            if (args.PropertyName is not nameof(vm.CurrentNovelIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentNovelIndex

            //if (EntryViewerSplitView.PinPane)
            //    EntryViewerSplitView.NavigationViewSelect(vm.Tags[0]);

            // todo Set Control ViewModel
        };

        _viewModel.PropertyChanged += (sender, args) =>
        {
            var vm = sender.To<NovelViewerPageViewModel>();
            switch (args.PropertyName)
            {
                case nameof(NovelViewerPageViewModel.IsFullScreen):
                {
                    Window.AppWindow.SetPresenter(vm.IsFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
                    // 加载完之后设置标题栏
                    _ = Task.Delay(500).ContinueWith(_ => RaiseSetTitleBarDragRegion(), TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                }
            }
        };

        // 第一次_viewModel.CurrentNovelIndex变化时，还没有订阅事件，所以不会触发DetailedPropertyChanged，需要手动触发
        // TODO
    }

    private void Placeholder_OnSizeChanged(object sender, object e) => RaiseSetTitleBarDragRegion();

    private void OpenPane_OnRightTapped(object sender, RightTappedRoutedEventArgs e) => EntryViewerSplitView.PinPane = true;

    private void Image_OnLoading(FrameworkElement sender, object args)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }
}
