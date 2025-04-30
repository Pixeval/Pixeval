// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class ImageViewerPage
{
    private ImageViewerPageViewModel _viewModel = null!;

    public ImageViewerPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (parameter is ImageViewerPageViewModel viewModel)
            _viewModel = viewModel;
        ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
    }

    private void OnImageViewerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ZoomableImage.ImageScale))
        {
            _viewModel.Scale = ImageViewer.ImageScale;
        }
    }

    public override void CompleteDisposal()
    {
        base.CompleteDisposal();
        Bindings.StopTracking();
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        // TODO 可能的内存泄露：多个页面共用一个vm
        Bindings.StopTracking();
    }
}
