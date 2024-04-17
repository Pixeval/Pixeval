using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage : IScrollViewProvider
{
    public RecommendUsersPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.RecommendIllustrators());
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
