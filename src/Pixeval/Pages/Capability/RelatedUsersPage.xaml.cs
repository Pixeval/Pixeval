// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Runtime;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class RelatedUsersPage : IScrollViewHost
{
    public RelatedUsersPage() => InitializeComponent();

    public override async void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;

        var engine = App.AppViewModel.MakoClient.Computed((await App.AppViewModel.MakoClient.RelatedUserAsync(userId, App.AppViewModel.AppSettings.TargetFilter))
            .ToAsyncEnumerable());
        IllustratorView.ViewModel.ResetEngine(engine);
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
