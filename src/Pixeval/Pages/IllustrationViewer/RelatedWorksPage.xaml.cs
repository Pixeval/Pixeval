using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class RelatedWorksPage
{
    private IllustrationViewerPageViewModel? _illustrationViewerPageViewModel;

    public RelatedWorksPage()
    {
        InitializeComponent();
    }

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        // Dispose current page contents if the parent page (IllustrationViewerPage) is navigating
        WeakReferenceMessenger.Default.TryRegister<RelatedWorksPage, NavigatingFromIllustrationViewerMessage>(this, (recipient, _) =>
        {
            recipient.RelatedWorksIllustrationGrid.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        });
        if (_illustrationViewerPageViewModel is null)
        {
            _illustrationViewerPageViewModel = e.Parameter as IllustrationViewerPageViewModel;
            await RelatedWorksIllustrationGrid.ViewModel.VisualizationController.ResetAndFillAsync(App.AppViewModel.MakoClient.RelatedWorks(_illustrationViewerPageViewModel!.IllustrationId));
        }
    }

    private void RelatedWorksIllustrationGrid_OnItemTapped(object? sender, IllustrationViewModel e)
    {
        IllustrationViewerPage.NavigatingStackEntriesFromRelatedWorksStack.Push((_illustrationViewerPageViewModel!.IllustrationId, _illustrationViewerPageViewModel.IsManga ? _illustrationViewerPageViewModel.CurrentIndex : null));
    }
}