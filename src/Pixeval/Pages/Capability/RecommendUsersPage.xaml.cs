// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Model;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage : IScrollViewHost
{
    public RecommendUsersPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        IllustratorView.ViewModel.ResetEngine(await App.AppViewModel.GetEngineAsync<User>("recommendation/users"));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
