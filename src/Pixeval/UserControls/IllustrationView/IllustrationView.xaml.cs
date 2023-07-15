#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RiverFlowIllustrationView.xaml.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.Converters;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Util.UI.Windowing;
using Windows.System;
using Windows.UI.Core;
using Pixeval.Util.Ref;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.IllustrationView;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty<IllustrationViewOption>("IllustrationViewOption", DependencyPropertyDefaultValue.Default, nameof(OnIllustrationViewOptionChanged))]
[DependencyProperty<ThumbnailDirection>("ThumbnailDirection", DependencyPropertyDefaultValue.Default, nameof(OnThumbnailDirectionChanged))]
public sealed partial class IllustrationView
{
    private static void OnIllustrationViewOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var value = e.NewValue.To<IllustrationViewOption>();
        d.To<UserControl>().Resources[nameof(Box)].To<Box>().Value = value.ToThumbnailUrlOption();
    }

    private static void OnThumbnailDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var value = e.NewValue.To<ThumbnailDirection>();
        d.To<UserControl>().Resources[nameof(Box)].To<Box>().Tag = value;
    }

    public IllustrationView()
    {
        InitializeComponent();
        _viewModelRef = new(new(), this);
        ViewModel.DataProvider.FilterChanged += (sender, _) =>
        {
            if (sender is Predicate<object> predicate)
                ViewModel.DataProvider.View.Filter = predicate;
            else
                ViewModel.DataProvider.View.Refresh();
            LoadMoreIfNeeded().Discard();
        };
    }

    public event EventHandler<IllustrationViewModel>? ItemTapped;

    private readonly SharedRef<IllustrationViewViewModel> _viewModelRef;

    public IllustrationViewViewModel ViewModel => _viewModelRef.Value;

    private void IllustrationViewOnUnloaded(object sender, RoutedEventArgs e)
    {
        var option = IllustrationViewOption.ToThumbnailUrlOption();
        foreach (var illustrationViewModel in ViewModel.DataProvider.Source)
            illustrationViewModel.UnloadThumbnail(ViewModel, option);
        _ = _viewModelRef.Dispose(this);
    }

    private async void ToggleBookmarkButtonOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<IllustrationViewModel>();
        if (viewModel.IsBookmarked)
            await viewModel.RemoveBookmarkAsync();
        else
            await viewModel.PostPublicBookmarkAsync();
    }

    private void ThumbnailOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
        {
            // User is doing the range selection
            return;
        }

        e.Handled = true;

        var vm = sender.GetDataContext<IllustrationViewModel>();
        ItemTapped?.Invoke(this, vm);

        var (width, height) = DetermineWindowSize(vm.Illustrate.Width, vm.Illustrate.Width / (double)vm.Illustrate.Height);

        var index = ViewModel.DataProvider.View.Cast<IllustrationViewModel>().ToList().IndexOf(vm);

        WindowFactory.RootWindow.Fork(out var w)
            .WithLoaded((o, _) => o.To<Frame>().NavigateTo<IllustrationViewerPage>(w,
                new IllustrationViewerPageViewModel(_viewModelRef, index),
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(new(width, height))
            .Activate();

        static (int windowWidth, int windowHeight) DetermineWindowSize(int illustWidth, double illustRatio)
        {
            /*
            var windowHandle = User32.MonitorFromWindow((nint)CurrentContext.HWnd, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
            User32.GetMonitorInfo(windowHandle, out var monitorInfoEx);
            var devMode = DEVMODE.Create();
            while (!User32.EnumDisplaySettings(
                       monitorInfoEx.DeviceName,
                       User32.ENUM_CURRENT_SETTINGS,
                       &devMode))
            { }

            var monitorWidth = devMode.dmPelsWidth;
            var monitorHeight = devMode.dmPelsHeight;
            */

            var (monitorWidth, monitorHeight) = WindowHelper.GetScreenSize();

            var determinedWidth = illustWidth switch
            {
                not 1500 => 1500 + Random.Shared.Next(0, 200),
                _ => 1500
            };
            var windowWidth = determinedWidth > monitorWidth ? (int)monitorWidth - 100 : determinedWidth;
            // 51 is determined through calculation, it is the height of the title bar
            var windowHeight = windowWidth / illustRatio + 51 is var height && height > monitorHeight - 80 // 80: estimated working area height
                ? monitorHeight - 100
                : height;
            return (windowWidth, (int)windowHeight);
        }
    }

    /// <summary>
    /// For Bookmark Button
    /// </summary>
    private void IllustrationThumbnailContainerItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private async void IllustrationThumbnailContainerItemOnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.GetDataContext<IllustrationViewModel>();
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);
        var option = IllustrationViewOption.ToThumbnailUrlOption();

        if (args.BringIntoViewDistanceY <= sender.ActualHeight * preLoadRows)
        {
            if (await context.TryLoadThumbnail(ViewModel, option))
            {
                if (sender.IsFullyOrPartiallyVisible(this))
                    sender.Resources["IllustrationThumbnailContainerItemStoryboard"].To<Storyboard>().Begin();
                else
                    sender.Opacity = 1;
            }
        }
        else
        {
            context.UnloadThumbnail(ViewModel, option);
        }
    }

    private void ScrollViewerViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        LoadMoreIfNeeded().Discard();
    }

    public async Task LoadMoreIfNeeded(uint number = 20)
    {
        // TODO load after being Filtrated
        if (ScrollViewer.ScrollableHeight - LoadingArea.ActualHeight < ScrollViewer.VerticalOffset)
            _ = await ViewModel.DataProvider.View.LoadMoreItemsAsync(number);
    }

    private async void BookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<IllustrationViewModel>().SwitchBookmarkStateAsync();
    }

    private async void SaveContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<IllustrationViewModel>().SaveAsync();
    }

    private async void SaveAsContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<IllustrationViewModel>().SaveAsAsync();
    }

    private async void OpenInBrowserContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id));
    }

    private void AddToBookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyWebLinkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.ClipboardSetText(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
    }

    private void CopyAppLinkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
    }

    private async void ShowQrCodeContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        var webQrCodeSource = await UIHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
        QrCodeTeachingTip.HeroContent.To<Image>().Source = webQrCodeSource;
        QrCodeTeachingTip.IsOpen = true;

        void Closed(TeachingTip s, TeachingTipClosedEventArgs ea)
        {
            webQrCodeSource.Dispose();
            s.Closed -= Closed;
        }

        QrCodeTeachingTip.Closed += Closed;
    }

    private async void ShowPixEzQrCodeContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        var pixEzQrCodeSource = await UIHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationPixEzUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
        QrCodeTeachingTip.HeroContent.To<Image>().Source = pixEzQrCodeSource;
        QrCodeTeachingTip.IsOpen = true;

        void Closed(TeachingTip s, TeachingTipClosedEventArgs ea)
        {
            pixEzQrCodeSource.Dispose();
            s.Closed -= Closed;
        }

        QrCodeTeachingTip.Closed += Closed;
    }
}
