// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadFolder : UserControl
{
    public event Action<DownloadFolder, DownloadFolderViewModel>? OpenRequested;

    public DownloadFolder() => InitializeComponent();

    private void Button_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DownloadFolderViewModel vm)
            OpenRequested?.Invoke(this, vm);
    }
}
