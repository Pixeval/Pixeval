// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Mako.Model;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage : IScrollViewHost
{
    public FollowingsPage() => InitializeComponent();

    private long _uid = -1;

    private bool IsMe => _uid == App.AppViewModel.PixivUid;

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _uid = uid;
        ChangeSource();
    }

    private void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();


    private async void ChangeSource()
    {
        IllustratorView.ViewModel.ResetEngine(await App.AppViewModel.GetEngineAsync<User>("follow/list", _uid));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
