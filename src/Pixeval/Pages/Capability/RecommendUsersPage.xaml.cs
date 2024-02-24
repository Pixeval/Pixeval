using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Pages.IllustratorViewer;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage
{
    public RecommendUsersPage() => InitializeComponent();

    private TeachingTip RecommendIllustratorItemOnRequestTeachingTip() => IllustrateView.QrCodeTeachingTip;

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;
        var recommendIllustrators = await App.AppViewModel.MakoClient.GetRelatedRecommendUsersAsync(userId, isR18: !App.AppViewModel.AppSettings.FiltrateRestrictedContent, lang: CultureInfo.CurrentUICulture);
        var viewModels = recommendIllustrators.ResponseBody.RecommendMaps
            .Select(ru => ToRecommendIllustratorProfileViewModel(recommendIllustrators, ru)).ToArray();

        IllustrateView.HasNoItem = viewModels.Length is 0;
        AdvancedItemsView.ItemsSource = viewModels;
        return;

        static RecommendIllustratorItemViewModel ToRecommendIllustratorProfileViewModel(PixivRelatedRecommendUsersResponse context, RecommendMap recommendUser)
        {
            var users = context.ResponseBody.Users;
            var userId = recommendUser.UserId;
            var user = users.First(u => u.Id == userId);
            return new RecommendIllustratorItemViewModel(user, recommendUser.IllustIds);
        }
    }

    private async void IllustratorItem_OnViewModelChanged(RecommendIllustratorItem sender, RecommendIllustratorItemViewModel viewModel)
    {
        await viewModel.LoadAvatarAsync();
    }

    private async void IllustratorItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(e.InvokedItem.To<RecommendIllustratorItemViewModel>().UserId);
    }
}
