// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class ImageViewerPage
{
    private ImageViewerPageViewModel _viewModel = null!;

    public ImageViewerPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (parameter is ImageViewerPageViewModel viewModel)
            _viewModel = viewModel;
    }
}
