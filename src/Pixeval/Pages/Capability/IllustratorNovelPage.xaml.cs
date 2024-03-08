using Microsoft.UI.Xaml.Controls;
using Pixeval.Misc;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Pages.Capability;

public sealed partial class IllustratorNovelsPage : IScrollViewProvider
{
    public IllustratorNovelsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is long id)
            NovelContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.NovelPosts(id, App.AppViewModel.AppSettings.TargetFilter));
    }

    public ScrollView ScrollView => NovelContainer.ScrollView;
}
