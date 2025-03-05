// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.IllustratorViewer;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class IllustratorView : IScrollViewHost, IStructuralDisposalCompleter
{
    public IllustratorViewViewModel ViewModel { get; } = new();

    public IllustratorView() => InitializeComponent();

    private TeachingTip IllustratorItemOnRequestTeachingTip() => IllustrateView.QrCodeTeachingTip;

    private async void IllustratorItem_OnViewModelChanged(IllustratorItem item, IllustratorItemViewModel viewModel)
    {
        await viewModel.LoadAvatarAsync();
    }

    private async void IllustratorItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(e.InvokedItem.To<IllustratorItemViewModel>().UserId);
    }

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;

    public void CompleteDisposal()
    {
        ViewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    private void IllustratorView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((IStructuralDisposalCompleter) this).Hook();
    }
}
