using Pixeval.UserControls;

namespace Pixeval.Pages.IllustrationViewer;

public interface IIllustrationVisualizer
{
    /// <summary>
    /// Dispose current visualizing illustrations, behaves like Clear
    /// </summary>
    void DisposeCurrent();

    /// <summary>
    /// Add a illustration view model to visualizer
    /// </summary>
    /// <param name="viewModel">The view model, usually fetched from FetchEngine</param>
    void AddIllustrationViewModel(IllustrationViewModel viewModel);
}