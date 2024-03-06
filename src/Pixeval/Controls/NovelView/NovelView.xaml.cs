using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls;

public sealed partial class NovelView : IEntryView<NovelViewViewModel>
{
    public NovelView() => InitializeComponent();

    public NovelViewViewModel ViewModel { get; } = new();

    public AdvancedItemsView AdvancedItemsView => NovelItemsView;

    public ScrollView ScrollView => NovelItemsView.ScrollView;

    private async void NovelItem_OnViewModelChanged(NovelItem sender, NovelItemViewModel viewModel)
    {
        await viewModel.TryLoadThumbnailAsync(ViewModel);
    }

    private void NovelItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
    }
}
