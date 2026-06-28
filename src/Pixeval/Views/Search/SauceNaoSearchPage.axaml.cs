using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Pixeval.Utilities;
using Pixeval.ViewModels.Search;
using Pixeval.Views.Work;

namespace Pixeval.Views.Search;

public partial class SauceNaoSearchPage : ContentPage
{
    public SauceNaoSearchPage()
    {
        InitializeComponent();
    }

    private async void OpenFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SauceNaoSearchPageViewModel viewModel
            || TopLevel.GetTopLevel(this) is not { StorageProvider: { } provider })
            return;
        if (await provider.OpenFilePickerAsync(new FilePickerOpenOptions() { AllowMultiple = false }) is not [{ } file])
            return;
        await using var stream = await file.OpenReadAsync();
        await viewModel.LoadAsync(stream);
    }

    private void SearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SauceNaoSearchPageViewModel viewModel
            || TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;
        viewContainer.NavigateTo(new ArtworkSauceNaoSearchResultPage(SauceNaoSearchPageViewModel.ApiKey, viewModel.File));
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is SauceNaoSearchPageViewModel vm)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
    }

    #endregion
}
