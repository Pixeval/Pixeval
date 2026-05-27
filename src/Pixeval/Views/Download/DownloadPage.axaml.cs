// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadPage : NavigationPage
{
    public DownloadPage()
    {
        DataContext = new DownloadViewViewModel(App.AppViewModel.HistoryPersistHelper.DownloadManager.QueuedTasks);
        InitializeComponent();
        _ = PushAsync(new DownloadView());
    }
}
