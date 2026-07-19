// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadPage : IconTabbedPage, IDisposable
{
    private readonly DownloadPageViewModel _viewModel;

    private readonly DownloadItemView _ordinaryView;

    private readonly DownloadFolderView _folderView;

    private bool _isDisposed;

    public DownloadPage()
    {
        InitializeComponent();
        _viewModel = new DownloadPageViewModel(
            App.AppViewModel.HistoryPersistHelper.DownloadManager.QueuedTasks,
            App.AppViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>());
        DataContext = _viewModel;

        _ordinaryView = new(new DownloadItemPageViewModel(_viewModel));
        _folderView = new(new DownloadFolderPageViewModel(_viewModel));
        Pages = [_ordinaryView, _folderView];
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!_isDisposed)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        _ordinaryView.Dispose();
        _folderView.Dispose();
        _viewModel.Dispose();
    }
}
