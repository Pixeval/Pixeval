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
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// <see cref="FrameworkElement.Unloaded"/>时会尝试卸载缩略图并释放FetchEngine
/// </summary>
[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default)]
[DependencyProperty<ThumbnailDirection>("ThumbnailDirection", DependencyPropertyDefaultValue.Default)]
public sealed partial class IllustrationView : IEntryView<IllustrationViewViewModel>
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

    public IllustrationViewViewModel ViewModel { get; } = new();

    public AdvancedItemsView AdvancedItemsView => IllustrationItemsView;

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

    public IllustrationView() => InitializeComponent();

    private void IllustrationViewOnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var illustrationViewModel in ViewModel.DataProvider.Source)
            illustrationViewModel.UnloadThumbnail(ViewModel);
        ViewModel.Dispose();
    }

    private (ThumbnailDirection ThumbnailDirection, double DesiredHeight) IllustrationItem_OnRequiredParam() => (ThumbnailDirection, DesiredHeight);

    private void IllustrationItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        var vm = e.InvokedItem.To<IllustrationItemViewModel>();

        vm.CreateWindowWithPage(ViewModel);
    }

    private async void IllustrationItem_OnViewModelChanged(IllustrationItem sender, IllustrationItemViewModel viewModel)
    {
        if (await viewModel.TryLoadThumbnailAsync(ViewModel))
        {
            if (sender.IsFullyOrPartiallyVisible(this))
                sender.Resources["IllustrationThumbnailStoryboard"].To<Storyboard>().Begin();
            else
                sender.Opacity = 1;
        }
    }

    private void IllustrationItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ViewModel.SelectedEntries = sender.SelectedItems.Cast<IllustrationItemViewModel>().ToArray();
    }

    private TeachingTip IllustrationItem_OnRequestTeachingTip() => IllustrateView.QrCodeTeachingTip;
}
