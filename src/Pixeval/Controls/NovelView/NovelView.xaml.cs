using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.NovelViewer;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class NovelView : IEntryView<NovelViewViewModel>
{
    public NovelView() => InitializeComponent();

    public NovelViewViewModel ViewModel { get; } = new();

    public AdvancedItemsView AdvancedItemsView => NovelItemsView;

    public ScrollView ScrollView => NovelItemsView.ScrollView;

    private async void NovelItem_OnViewModelChanged(NovelItem sender, NovelItemViewModel viewModel)
    {
        _ = await viewModel.TryLoadThumbnailAsync(ViewModel);
    }

    private void NovelItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        var viewModel = e.InvokedItem.To<NovelItemViewModel>();

        viewModel.CreateWindowWithPage(ViewModel);
    }
}
