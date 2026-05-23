// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Download;

public partial class DownloadView : UserControl
{
    public DownloadView() => InitializeComponent();

    private async void DownloadItem_OnOpenIllustrationRequested(DownloadItem sender, DownloadItemViewModel viewModel)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.Entry is Novel novel)
            await viewContainer.CreateNovelPageAsync(novel.Id);
        else
            await viewContainer.CreateIllustrationPageAsync(viewModel.Entry);
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is DownloadViewViewModel vm)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
    }

    #endregion
}
