// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;

namespace Pixeval.Pages.Capability.Feeds;

public sealed partial class CondensedFeedPage
{
    public CondensedFeedPage()
    {
        InitializeComponent();
        WorkView.LayoutType = App.AppViewModel.AppSettings.ItemsViewLayoutType;
        WorkView.ThumbnailDirection = App.AppViewModel.AppSettings.ThumbnailDirection;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        switch (e.Parameter)
        {
            case IEnumerable<Illustration> works:
                WorkView.ResetEngine(App.AppViewModel.MakoClient.Computed(works.ToAsyncEnumerable()));
                break;
            case IEnumerable<Novel> works2:
                WorkView.ResetEngine(App.AppViewModel.MakoClient.Computed(works2.ToAsyncEnumerable()));
                break;
        }
    }
}
