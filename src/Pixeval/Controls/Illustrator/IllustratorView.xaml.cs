// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.IllustratorViewer;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class IllustratorView : IScrollViewHost
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

    ~IllustratorView() => ViewModel.Dispose();

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;
}
