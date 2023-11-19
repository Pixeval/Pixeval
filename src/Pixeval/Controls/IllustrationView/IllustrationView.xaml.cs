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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustrationView;

// use "load failed" image for those thumbnails who failed to load its source due to various reasons
// note: please ALWAYS add e.Handled = true before every "tapped" event for the buttons
[DependencyProperty<IllustrationViewOption>("IllustrationViewOption", DependencyPropertyDefaultValue.Default)]
[DependencyProperty<ThumbnailDirection>("ThumbnailDirection", DependencyPropertyDefaultValue.Default, nameof(OnThumbnailDirectionChanged))]
[INotifyPropertyChanged]
public sealed partial class IllustrationView
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

    public double DesiredHeight => ThumbnailDirection switch
    {
        ThumbnailDirection.Landscape => LandscapeHeight,
        ThumbnailDirection.Portrait => PortraitHeight,
        _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(ThumbnailDirection)
    };

    private static void OnThumbnailDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<IllustrationView>().OnPropertyChanged(nameof(ThumbnailDirection));
    }

    public IllustrationView()
    {
        InitializeComponent();
        ViewModel.DataProvider.FilterChanged += (_, filter) =>
        {
            if (filter is { } predicate)
                ViewModel.DataProvider.View.Filter = o => predicate((IllustrationViewModel)o);
            else
                ViewModel.DataProvider.View.Refresh();
            LoadMoreIfNeeded().Discard();
        };
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
        var option = IllustrationViewOption.ToThumbnailUrlOption();
        foreach (var illustrationViewModel in ViewModel.DataProvider.Source)
            illustrationViewModel.UnloadThumbnail(ViewModel, option);
        ViewModel.Dispose();
    }

    private async void IllustrationThumbnailContainerItemOnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var context = sender.To<IllustrationThumbnail>().ViewModel;
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);
        var option = IllustrationViewOption.ToThumbnailUrlOption();

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

    private void ScrollViewerViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        LoadMoreIfNeeded().Discard();
    }

    public async Task LoadMoreIfNeeded(uint number = 20)
    {
        // TODO load after being Filtrated
        if (ScrollViewer.ScrollableHeight - LoadingArea.ActualHeight < ScrollViewer.VerticalOffset)
            _ = await ViewModel.DataProvider.LoadMoreAsync(number);
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
}
