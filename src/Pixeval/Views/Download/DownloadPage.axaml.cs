// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadPage : IconNavigationPage
{
    public DownloadPage()
    {
        DataContext = new DownloadViewViewModel(App.AppViewModel.HistoryPersistHelper.DownloadManager.QueuedTasks);
        InitializeComponent();
        _ = PushAsync(new DownloadView());
    }
}
