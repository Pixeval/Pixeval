// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadFolderView : NavigationPage, IDisposable
{
    private readonly DownloadFolderPageViewModel? _viewModel;

    private bool _isDisposed;

    public DownloadFolderView() => InitializeComponent();

    public DownloadFolderView(DownloadFolderPageViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        PageRemoved += PageRemoved_On;
        _ = PushAsync(new DownloadFolderListView(viewModel));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!_isDisposed)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    private static void PageRemoved_On(object? sender, PageRemovedEventArgs e)
    {
        if (e.Page is IDisposable disposable)
            disposable.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        PageRemoved -= PageRemoved_On;
        foreach (var page in NavigationStack.OfType<IDisposable>().ToArray())
            page.Dispose();
        _viewModel?.Dispose();
    }
}
