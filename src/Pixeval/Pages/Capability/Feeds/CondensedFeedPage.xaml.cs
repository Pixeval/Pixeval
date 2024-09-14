using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Engine;
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

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is IEnumerable<IWorkEntry> works)
        {
            WorkView.ResetEngine(App.AppViewModel.MakoClient.Computed(works.ToAsyncEnumerable()));
        }
    }
}
