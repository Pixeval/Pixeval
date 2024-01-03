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

using System.Linq;
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
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;
    public ThumbnailUrlOption Option => LayoutType.ToThumbnailUrlOption();

    public IllustrationView()
    {
        InitializeComponent();
        ViewModel.DataProvider.View.FilterChanged += async (_, _) => await IllustrationItemsView.TryRaiseLoadMoreRequestedAsync();
    }

    public ScrollView ScrollView => IllustrationItemsView.ScrollView;

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
        foreach (var illustrationViewModel in ViewModel.DataProvider.Source)
            illustrationViewModel.UnloadThumbnail(ViewModel, Option);
        ViewModel.Dispose();
    }

    public async void LoadMoreIfNeeded()
    {
        await IllustrationItemsView.TryRaiseLoadMoreRequestedAsync();
    }

    private IllustrationView IllustrationThumbnail_OnThisRequired() => this;

    private void IllustrationItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        var vm = e.InvokedItem.To<IllustrationItemViewModel>();

        vm.CreateWindowWithPage(ViewModel);
    }

    private async void IllustrationItemsView_OnElementPrepared(AdvancedItemsView sender, ItemContainer itemContainer)
    {
        var thumbnail = itemContainer.Child.To<IllustrationItem>();
        var viewModel = thumbnail.ViewModel;

        if (await viewModel.TryLoadThumbnail(ViewModel, Option))
        {
            if (thumbnail.IsFullyOrPartiallyVisible(this))
                thumbnail.Resources["IllustrationThumbnailStoryboard"].To<Storyboard>().Begin();
            else
                thumbnail.Opacity = 1;
        }
    }

    private void IllustrationItemsView_OnElementClearing(AdvancedItemsView sender, ItemContainer itemContainer)
    {
        var viewModel = itemContainer.Child.To<IllustrationItem>().ViewModel;

        viewModel.UnloadThumbnail(ViewModel, Option);
    }

    private void IllustrationItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ViewModel.SelectedIllustrations = sender.SelectedItems.Cast<IllustrationItemViewModel>().ToArray();
    }
}
