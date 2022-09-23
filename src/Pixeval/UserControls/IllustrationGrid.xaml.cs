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
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Attributes;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.UserControls;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty("Header", typeof(object))]
public sealed partial class IllustrationGrid
{
    private static readonly ExponentialEase ImageSourceSetEasingFunction = new()
    {
        EasingMode = EasingMode.EaseOut,
        Exponent = 12
    };

    public IllustrationGrid()
    {
        InitializeComponent();
        ViewModel = new IllustrationGridViewModel();
    }

    private EventHandler<IllustrationViewModel>? _itemTapped;

    public event EventHandler<IllustrationViewModel> ItemTapped
    {
        add => _itemTapped += value;
        remove => _itemTapped -= value;
    }

    public IllustrationGridViewModel ViewModel { get; }

    private void IllustrationGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        switch (App.AppViewModel.AppSetting.ThumbnailDirection)
        {
            case ThumbnailDirection.Landscape:
                IllustrationGridView.ItemHeight = 180;
                IllustrationGridView.DesiredWidth = 250;
                break;
            case ThumbnailDirection.Portrait:
                IllustrationGridView.ItemHeight = 250;
                IllustrationGridView.DesiredWidth = 180;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<IllustrationViewModel>();
        await viewModel.RemoveBookmarkAsync();
    }

    private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<IllustrationViewModel>();
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

        _itemTapped?.Invoke(this, sender.GetDataContext<IllustrationViewModel>());

        var viewModels = sender.GetDataContext<IllustrationViewModel>()
            .GetMangaIllustrationViewModels()
            .ToArray();

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (UIElement)sender);
        App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(this, viewModels), new SuppressNavigationTransitionInfo());
    }

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
            if (await context.LoadThumbnailIfRequired())
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

    public UIElement? GetItemContainer(IllustrationViewModel viewModel)
    {
        return IllustrationGridView.ContainerFromItem(viewModel) as UIElement;
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
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString()));
    }

    private void CopyAppLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationAppUri(sender.GetDataContext<IllustrationViewModel>().Id).ToString()));
    }

    private async void ShowQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowQrCodeForIllustrationAsync(sender.GetDataContext<IllustrationViewModel>());
    }

    private async void ShowPixEzQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowPixEzQrCodeForIllustrationAsync(sender.GetDataContext<IllustrationViewModel>());
    }
}