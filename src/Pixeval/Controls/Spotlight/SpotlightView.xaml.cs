// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using System;

namespace Pixeval.Controls;

public sealed partial class SpotlightView : IScrollViewHost
{
    public SpotlightViewViewModel ViewModel { get; } = new();

    public SpotlightView()
    {
        InitializeComponent();
    }

    private async void SpotlightItem_OnViewModelChanged(SpotlightItem item, SpotlightItemViewModel viewModel)
    {
        await viewModel.TryLoadThumbnailAsync(ViewModel);
    }

    private async void IllustratorItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri(e.InvokedItem.To<SpotlightItemViewModel>().Entry.ArticleUrl));
    }

    ~SpotlightView()
    {
        foreach (var viewModel in ViewModel.DataProvider.Source)
            viewModel.UnloadThumbnail(ViewModel);
        ViewModel.Dispose();
    }

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;
}
