// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Pixeval.AppManagement;

namespace Pixeval.Views.Settings;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        LoadData();
    }

    public ObservableCollection<Supporter> Supporters { get; } = [];

    private async void LoadData()
    {
        LicenseTextBlock.Markdown = Encoding.UTF8.GetString(await AppInfo.GetAssetBytesAsync("GPLv3.md"));
        await foreach (var supporter in Supporter.GetSupportersAsync())
            Supporters.Add(supporter);
    }

    private void SupporterCard_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { Tag: Uri uri })
            _ = TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
    }
}
