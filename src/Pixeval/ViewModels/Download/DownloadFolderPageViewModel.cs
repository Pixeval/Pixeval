// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public sealed class DownloadFolderPageViewModel : ViewModelBase, IDisposable
{
    private bool _isDisposed;

    public DownloadPageViewModel PageViewModel { get; }

    public AdvancedObservableCollection<DownloadFolderViewModel> View { get; }

    public DownloadFolderPageViewModel(DownloadPageViewModel pageViewModel)
    {
        PageViewModel = pageViewModel;
        View = new AdvancedObservableCollection<DownloadFolderViewModel>(pageViewModel.SubscriptionFolders, true);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        View.Dispose();
    }
}
