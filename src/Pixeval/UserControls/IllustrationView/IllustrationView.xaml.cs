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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Util.UI.Windowing;
using Windows.System;
using Windows.UI.Core;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.IllustrationView;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty<IllustrationViewOption>("IllustrationViewOption")]
public sealed partial class IllustrationView
{
    private static readonly ExponentialEase _imageSourceSetEasingFunction = new()
    {
        EasingMode = EasingMode.EaseOut,
        Exponent = 12
    };

    public IllustrationView()
    {
        InitializeComponent();
        ViewModel = new IllustrationViewViewModel();
        ViewModel.DataProvider.FilterChanged += (sender, _) =>
        {
            if (sender is Predicate<object> predicate)
            {
                ViewModel.DataProvider.View.Filter = predicate;
            }
            else
            {
                ViewModel.DataProvider.View.Refresh();
            }
            LoadMoreIfNeeded().Discard();
        };
    }

    public event EventHandler<IllustrationViewModel>? ItemTapped;

    public IllustrationViewViewModel ViewModel { get; }

    private async void ToggleBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<IllustrationViewModel>();
        if (viewModel.IsBookmarked)
            await viewModel.RemoveBookmarkAsync();
        else
            await viewModel.PostPublicBookmarkAsync();
    }

    private void Thumbnail_OnTapped(object sender, TappedRoutedEventArgs e)
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

        var illustrations = ViewModel.DataProvider.Source.ToArray();
        var index = Array.IndexOf(illustrations, vm);


        WindowFactory.RootWindow.Fork(out var w)
            .WithLoaded((o, _) => o.To<Frame>().NavigateTo<IllustrationViewerPage>(w,
                new IllustrationViewerPageViewModel(illustrations, index, ViewModel.DataProvider.View.Cast<IllustrationViewModel>()),
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(new(width, height))
            .Activate();
    }

    private static (int windowWidth, int windowHeight) DetermineWindowSize(int illustWidth, double illustRatio)
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

    /// <summary>
    /// For Bookmark Button
    /// </summary>
    private void IllustrationThumbnailContainerItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private async void IllustrationThumbnailContainerItem_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.GetDataContext<IllustrationViewModel>();
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);

        if (args.BringIntoViewDistanceY <= sender.ActualHeight * preLoadRows)
        {
            var option = IllustrationViewOption switch
            {
                IllustrationViewOption.RiverFlow => ThumbnailUrlOption.Medium,
                IllustrationViewOption.Grid => ThumbnailUrlOption.SquareMedium,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<IllustrationViewOption, ThumbnailUrlOption>(IllustrationViewOption)
            };
            if (await context.LoadThumbnailIfRequired(option))
            {
                var transform = (ScaleTransform)sender.RenderTransform;
                if (sender.IsFullyOrPartiallyVisible(this))
                {
                    var scaleXAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleX), from: 1.1, to: 1, easingFunction: _imageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                    var scaleYAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleY), from: 1.1, to: 1, easingFunction: _imageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                    var opacityAnimation = sender.CreateDoubleAnimation(nameof(sender.Opacity), from: 0, to: 1, easingFunction: _imageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                    UIHelper.CreateStoryboard(scaleXAnimation, scaleYAnimation, opacityAnimation).Begin();
                }
                else
                {
                    transform.ScaleX = 1;
                    transform.ScaleY = 1;
                    sender.Opacity = 1;
                }
            }

            return;
        }

        // small tricks to reduce memory consumption
        switch (context)
        {
            case { LoadingThumbnail: true }:
                context.LoadingThumbnailCancellationHandle.Cancel();
                break;
            case { ThumbnailSource: not null }:
                var source = context.ThumbnailSource;
                context.ThumbnailSource = null;
                source.Dispose();
                break;
        }
    }

    public async Task LoadMoreIfNeeded(uint number = 20)
    {
        // TODO load after being Filtrated
        if (ScrollViewer.ScrollableHeight - LoadingArea.ActualHeight < ScrollViewer.VerticalOffset)
            await ViewModel.DataProvider.View.LoadMoreItemsAsync(number);
    }

    private void BookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        sender.GetDataContext<IllustrationViewModel>().SwitchBookmarkStateAsync();
    }

    private async void SaveContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<IllustrationViewModel>().SaveAsync();
    }

    private async void SaveAsContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<IllustrationViewModel>().SaveAsAsync();
    }

    private async void OpenInBrowserContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id));
    }

    private void AddToBookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyWebLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.ClipboardSetText(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
    }

    private void CopyAppLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString());
    }

    private async void ShowQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
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

    private async void ShowPixEzQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
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

    private void ScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        LoadMoreIfNeeded().Discard();
    }
}
