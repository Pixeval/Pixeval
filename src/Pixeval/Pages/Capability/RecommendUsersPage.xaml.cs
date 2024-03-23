using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Misc;
using Pixeval.Pages.IllustratorViewer;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage : IScrollViewProvider
{
    public RecommendUsersPage() => InitializeComponent();

    private TeachingTip RecommendIllustratorItemOnRequestTeachingTip() => IllustrateView.QrCodeTeachingTip;

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;
        IllustrateView.IsLoadingMore = true;
        try
        {
            var users = (await App.AppViewModel.MakoClient.RelatedUserAsync(userId)).Users;
            var viewModels = users.Select(t => new IllustratorItemViewModel(t));

            IllustrateView.HasNoItem = users.Length is 0;
            AdvancedItemsView.ItemsSource = viewModels;
        }
        finally
        {
            IllustrateView.IsLoadingMore = false;
        }
    }

    private async void IllustratorItem_OnViewModelChanged(IllustratorItem sender, IllustratorItemViewModel viewModel)
    {
        await viewModel.LoadAvatarAsync();
    }

    private async void IllustratorItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(e.InvokedItem.To<IllustratorItemViewModel>().UserId);
    }

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;
}
