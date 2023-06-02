#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationGrid.xaml.cs
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
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;
using PInvoke;
using Pixeval.Util.UI.Windowing;
using Windows.Graphics;
using WinUI3Utilities;

namespace Pixeval.UserControls.IllustrationView;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty<object>("Header")]
public sealed partial class RiverFlowIllustrationView : IIllustrationView
{
    private bool _fillClientRequest;

    private static readonly ExponentialEase ImageSourceSetEasingFunction = new()
    {
        EasingMode = EasingMode.EaseOut,
        Exponent = 12
    };

    public RiverFlowIllustrationView()
    {
        InitializeComponent();
        ViewModel = new RiverFlowIllustrationViewViewModel();
        ViewModel.DataProvider.FilterChanged += (sender, _) =>
        {
            if (sender is Predicate<object> predicate)
            {
                ViewModel.DataProvider.IllustrationsView.Filter = predicate;
            }
            else
            {
                ViewModel.DataProvider.IllustrationsView.Refresh();
            }
            TryFillClientAreaAsync().Discard();
        };
    }

    private EventHandler<IllustrationViewModel>? _itemTapped;

    public event EventHandler<IllustrationViewModel> ItemTapped
    {
        add => _itemTapped += value;
        remove => _itemTapped -= value;
    }

    public RiverFlowIllustrationViewViewModel ViewModel { get; }

    public FrameworkElement SelfIllustrationView => this;

    IllustrationViewViewModel IIllustrationView.ViewModel => ViewModel;

    public ScrollViewer ScrollViewer => Sv;

    private void IllustrationGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        switch (App.AppViewModel.AppSetting.ThumbnailDirection)
        {
            case ThumbnailDirection.Landscape:
                RiverFlowLayout.LineSize = 180;
                break;
            case ThumbnailDirection.Portrait:
                RiverFlowLayout.LineSize = 250;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = UIHelper.GetDataContext<IllustrationViewModel>(sender);
        await viewModel.RemoveBookmarkAsync();
    }

    private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = UIHelper.GetDataContext<IllustrationViewModel>(sender);
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
        WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(sender as UIElement));

        _itemTapped?.Invoke(this, UIHelper.GetDataContext<IllustrationViewModel>(sender));

        var viewModels = UIHelper.GetDataContext<IllustrationViewModel>(sender)
            .GetMangaIllustrationViewModels()
            .ToArray();

        // This is commented because the connected animation used to be used when IllustrationViewerPage does not create a new window.
        // ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (UIElement) sender);

        // TODO: Test Use the new windowing API
        var (width, height) = DetermineWindowSize(viewModels[0].Illustration.Width, viewModels[0].Illustration.Width / (double)viewModels[0].Illustration.Height);
        var window = WindowFactory.Fork(
            new AppHelper.InitializeInfo { TitleBarType = TitleBarHelper.TitleBarType.AppWindow, Size = new SizeInt32(width, height) },
            CurrentContext.Window,
            onLoaded: (o, _) => o.To<Frame>().Navigate(typeof(IllustrationViewerPage),
                new IllustrationViewerPageViewModel(this, viewModels), new SuppressNavigationTransitionInfo()));
        window.Activate();
    }

    private static unsafe (int windowWidth, int windowHeight) DetermineWindowSize(int illustWidth, double illustRatio)
    {
        var windowPlacement = User32.WINDOWPLACEMENT.Create();
        User32.GetWindowPlacement(CurrentContext.Window.GetWindowHandle(), &windowPlacement);
        var windowHandle = User32.MonitorFromWindow(CurrentContext.Window.GetWindowHandle(), User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
        User32.GetMonitorInfo(windowHandle, out var monitorInfoEx);
        var devMode = DEVMODE.Create();
        while (!User32.EnumDisplaySettings(
                   monitorInfoEx.DeviceName,
                   User32.ENUM_CURRENT_SETTINGS,
                   &devMode)) { }

        var monitorWidth = devMode.dmPelsWidth;
        var monitorHeight = devMode.dmPelsHeight;

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

    private void IllustrationThumbnailContainerItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private async void IllustrationThumbnailContainerItem_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = UIHelper.GetDataContext<IllustrationViewModel>(sender);
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);

        if (args.BringIntoViewDistanceY <= sender.ActualHeight * preLoadRows)
        {
            if (await context.LoadThumbnailIfRequired(ThumbnailUrlOption.Large))
            {
                var transform = (ScaleTransform)sender.RenderTransform;
                if (sender.IsFullyOrPartiallyVisible(this))
                {
                    var scaleXAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleX), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                    var scaleYAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleY), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                    var opacityAnimation = sender.CreateDoubleAnimation(nameof(sender.Opacity), from: 0, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
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

    public async Task TryFillClientAreaAsync()
    {
        if (_fillClientRequest)
        {
            return;
        }
        _fillClientRequest = true;
        for (var i = ViewModel.DataProvider.IllustrationsView.Count - 1; i > 0; i--)
        {
            //TODO
            var container = IllustrationItemsRepeater.ItemsSourceView.GetAt(i) as FrameworkElement;
            if (!(container?.IsFullyOrPartiallyVisible(this) ?? true)) return;
        }

        var index = ViewModel.DataProvider.IllustrationsView.Count - 1;
        var acv = ViewModel.DataProvider.IllustrationsView;
        while (await acv.LoadMoreItemsAsync(20) is { Count: > 0 and var count })
        {
            for (var i = index + (int)count; i > index + 1; i--)
            {
                //TODO
                var container = IllustrationItemsRepeater.ItemsSourceView.GetAt(i) as FrameworkElement;
                if (!(container?.IsFullyOrPartiallyVisible(this) ?? true)) return;
            }

            index = (int)(index + count);
        }
    }

    public UIElement? GetItemContainer(IllustrationViewModel viewModel)
    {
        //TODO
        return null;// IllustrationItemsRepeater.ItemsSourceView.f as UIElement;
    }

    private void BookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.GetDataContext<IllustrationViewModel>(sender).SwitchBookmarkStateAsync();
    }

    private async void SaveContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await UIHelper.GetDataContext<IllustrationViewModel>(sender).SaveAsync();
    }

    private async void SaveAsContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await UIHelper.GetDataContext<IllustrationViewModel>(sender).SaveAsAsync();
    }

    private async void OpenInBrowserContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(UIHelper.GetDataContext<IllustrationViewModel>(sender).Id));
    }

    private void AddToBookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyWebLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationWebUri(UIHelper.GetDataContext<IllustrationViewModel>(sender).Id).ToString()));
    }

    private void CopyAppLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationAppUri(UIHelper.GetDataContext<IllustrationViewModel>(sender).Id).ToString()));
    }

    private async void ShowQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowQrCodeForIllustrationAsync(UIHelper.GetDataContext<IllustrationViewModel>(sender));
    }

    private async void ShowPixEzQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowPixEzQrCodeForIllustrationAsync(UIHelper.GetDataContext<IllustrationViewModel>(sender));
    }
}
