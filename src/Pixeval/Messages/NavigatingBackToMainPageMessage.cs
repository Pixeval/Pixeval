using Pixeval.Pages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.ViewModel;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event occurs when <see cref="IllustrationViewerPage"/> is about to navigate back to
    /// <see cref="MainPage"/>, the parameter contains the item that the <see cref="IllustrationViewerPage"/>
    /// is currently browsing
    /// </summary>
    public record NavigatingBackToMainPageMessage(IllustrationViewModel? IllustrationViewModel);
}