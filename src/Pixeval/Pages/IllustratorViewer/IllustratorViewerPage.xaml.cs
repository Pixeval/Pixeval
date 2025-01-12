// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Utilities;
using Pixeval.Controls.Windowing;
using Pixeval.Controls;
using Pixeval.Util.UI;

namespace Pixeval.Pages.IllustratorViewer;

public sealed partial class IllustratorViewerPage
{
    private IllustratorViewerPageViewModel _viewModel = null!;

    public IllustratorViewerPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => SetViewModel(parameter);

    public void SetViewModel(object? parameter)
    {
        _viewModel = this.GetIllustratorViewerPageViewModel(parameter);
    }

    private async void IllustratorViewerNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        var currentTag = e.SelectedItem.To<NavigationViewTag?>() ?? _viewModel.WorkTag;

        IllustratorViewerFrame.NavigateTag(currentTag, e.RecommendedNavigationTransitionInfo);
        _ = await IllustratorViewerFrame.AwaitPageTransitionAsync(currentTag.NavigateTo);
        StickyHeaderScrollView.RaiseSetInnerScrollView();
    }

    private double StickyHeaderScrollView_OnGetScrollableLength() => ScrollableLength.ActualHeight;

    private ScrollView? StickyHeaderScrollView_OnSetInnerScrollView()
    {
        return IllustratorViewerFrame.Content is IScrollViewHost { ScrollView: { } scrollView } ? scrollView : null;
    }
}
