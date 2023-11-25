#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationView.xaml.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustrationView;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default)]
[DependencyProperty<ThumbnailDirection>("ThumbnailDirection", DependencyPropertyDefaultValue.Default)]
public sealed partial class IllustrationView
{
    public ScrollView ScrollView => IllustrationItemsView.ScrollView;

    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

    public double DesiredHeight => ThumbnailDirection switch
    {
        ThumbnailDirection.Landscape => LandscapeHeight,
        ThumbnailDirection.Portrait => PortraitHeight,
        _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(ThumbnailDirection)
    };

    public double DesiredWidth => ThumbnailDirection switch
    {
        ThumbnailDirection.Landscape => PortraitHeight,
        ThumbnailDirection.Portrait => LandscapeHeight,
        _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(ThumbnailDirection)
    };

    public IllustrationView()
    {
        InitializeComponent();
        ViewModel.DataProvider.View.FilterChanged += (_, _) => IllustrationItemsView.TryRaiseLoadMoreRequested();
    }

    public IllustrationViewViewModel ViewModel { get; } = new();

    private void IllustrationThumbnailOnShowQrCodeRequested(object sender, SoftwareBitmapSource e)
    {
        QrCodeTeachingTip.HeroContent.To<Image>().Source = e;
        QrCodeTeachingTip.IsOpen = true;
        QrCodeTeachingTip.Closed += Closed;
        return;

        void Closed(TeachingTip s, TeachingTipClosedEventArgs ea)
        {
            e.Dispose();
            s.Closed -= Closed;
        }
    }

    private void IllustrationViewOnUnloaded(object sender, RoutedEventArgs e)
    {
        var option = LayoutType.ToThumbnailUrlOption();
        foreach (var illustrationViewModel in ViewModel.DataProvider.Source)
            illustrationViewModel.UnloadThumbnail(ViewModel, option);
        ViewModel.Dispose();
    }

    private async void IllustrationThumbnailContainerItemOnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.To<IllustrationThumbnail>().ViewModel;
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);
        var option = LayoutType.ToThumbnailUrlOption();

        if (args.BringIntoViewDistanceY <= sender.ActualHeight * preLoadRows)
        {
            if (await context.TryLoadThumbnail(ViewModel, option))
            {
                if (sender.IsFullyOrPartiallyVisible(this))
                    sender.Resources["IllustrationThumbnailStoryboard"].To<Storyboard>().Begin();
                else
                    sender.Opacity = 1;
            }
        }
        else
        {
            context.UnloadThumbnail(ViewModel, option);
        }
    }

    public void LoadMoreIfNeeded()
    {
        IllustrationItemsView.TryRaiseLoadMoreRequested();
    }

    private IllustrationView IllustrationThumbnail_OnThisRequired()
    {
        return this;
    }

    private void IllustrationItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        var vm = e.InvokedItem.To<IllustrationViewModel>();

        vm.CreateWindowWithPage(ViewModel);
    }

    private async Task IllustrationItemsView_OnLoadMoreRequested(object? sender, EventArgs e)
    {
        await ViewModel.LoadMoreAsync(20);
    }
}
