using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Misc;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class SearchUsersPage : IScrollViewHost
{
    public SearchUsersPage() => InitializeComponent();

    private string _searchText = "";

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _searchText = e.Parameter.To<string>();
        ChangeSource();
    }

    private void ChangeSource()
    {
        var settings = App.AppViewModel.AppSettings;
        IllustratorView.ViewModel.ResetEngine(
            App.AppViewModel.MakoClient.SearchUser(
                _searchText,
                settings.TargetFilter));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
