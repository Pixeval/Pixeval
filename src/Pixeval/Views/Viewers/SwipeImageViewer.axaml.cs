using System.Linq;
using Avalonia;
using Pixeval.Controls;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class SwipeImageViewer : SwipeControl
{
    public SwipeImageViewer() => InitializeComponent();

    public static readonly DirectProperty<SwipeImageViewer, SingleImageViewer?> CurrentPageProperty =
        AvaloniaProperty.RegisterDirect<SwipeImageViewer, SingleImageViewer?>(
            nameof(CurrentPage),
            o => o.CurrentPage);

    public SingleImageViewer? CurrentPage => Content as SingleImageViewer;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
            RaisePropertyChanged(CurrentPageProperty, null, CurrentPage);
    }

    private void SelectingMultiPage_OnSelectionChanged(SwipeControl sender, SwipeControlSelectionChangedEventArgs e)
    {
        if (sender.DataContext is not ImageViewerViewModel viewModel)
            return;

        var preloadPageCount = 2;
        var load = viewModel.Images.Skip(e.NewIndex - preloadPageCount).Take((preloadPageCount * 2) + 1);
        foreach (var loadableBitmap in load)
            _ = loadableBitmap.LoadOriginalImageAsync();
    }
}
