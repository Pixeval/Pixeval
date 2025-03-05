// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class SpotlightView : IScrollViewHost, IStructuralDisposalCompleter
{
    public SpotlightViewViewModel ViewModel { get; } = new();

    public SpotlightView()
    {
        InitializeComponent();
    }

    private async void SpotlightItem_OnViewModelChanged(SpotlightItem item, SpotlightItemViewModel viewModel)
    {
        _ = await viewModel.TryLoadThumbnailAsync(ViewModel);
    }

    private async void IllustratorItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(e.InvokedItem.To<SpotlightItemViewModel>().Entry.ArticleUrl));
    }

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;

    public void CompleteDisposal()
    {
        foreach (var viewModel in ViewModel.DataProvider.Source)
            viewModel.UnloadThumbnail(ViewModel);
        ViewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    private void SpotlightView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((IStructuralDisposalCompleter) this).Hook();
    }
}
