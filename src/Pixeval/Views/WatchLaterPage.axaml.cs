// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;

namespace Pixeval.Views;

public partial class WatchLaterPage : ContentPage
{
    public WatchLaterPage()
    {
        InitializeComponent();
        WorkContainer.SetSource(App.AppViewModel.HistoryPersistHelper.WatchLaterEntries);
    }
}