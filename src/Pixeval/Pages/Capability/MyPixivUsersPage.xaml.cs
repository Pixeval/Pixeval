using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Misc;

namespace Pixeval.Pages;

public sealed partial class MyPixivUsersPage : IScrollViewHost
{
    public MyPixivUsersPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;

        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.MyPixivUsers(userId));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
