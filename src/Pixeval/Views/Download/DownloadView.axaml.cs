using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Work;

namespace Pixeval.Views.Download;

public partial class DownloadView : UserControl, IStructuralDisposalCompleter
{
    public DownloadView() => InitializeComponent();

    private async void DownloadItem_OnOpenIllustrationRequested(DownloadItem sender, DownloadItemViewModel viewModel)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.Entry is Novel)
            ;// _ = await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(viewModel.Entry.WebsiteUri);
        else
            await viewContainer.CreateIllustrationPageAsync(viewModel.Entry);
    }

    private async void DownloadItem_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not DownloadViewViewModel viewModel)
            return;
        if (sender is not Control { DataContext: DownloadItemViewModel vm })
            return;

        _ = await vm.TryLoadThumbnailAsync(viewModel);
    }

    public void CompleteDisposal()
    {
        if (DataContext is DownloadViewViewModel viewModel)
            viewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ((IStructuralDisposalCompleter)this).Hook();
    }
}
