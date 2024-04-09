using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage : IScrollViewProvider
{
    public RecommendUsersPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;

        var engine = App.AppViewModel.MakoClient.Computed((await App.AppViewModel.MakoClient.RelatedUserAsync(userId))
            .Users.ToAsyncEnumerable());
        IllustratorView.ViewModel.ResetEngine(engine);
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
