using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;

namespace Pixeval.Pages;

public sealed partial class MyPixivPage : IScrollViewProvider
{
    public MyPixivPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;

        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.MyPixiv(userId));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
