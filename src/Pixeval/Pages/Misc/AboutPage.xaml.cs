// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Windows.System;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

/// <summary>
/// Page that contains the information about this application, including contributor, sponsor, open source library and open source license
/// </summary>
public sealed partial class AboutPage
{
    /// <summary>
    /// TODO add sponsors
    /// </summary>
    public AboutPage()
    {
        InitializeComponent();
        LoadData();
    }

    private ObservableCollection<Supporter> Supporters { get; } = [];

    private async void LoadData()
    {
        LicenseTextBlock.Text = Encoding.UTF8.GetString(await AppInfo.GetAssetBytesAsync("GPLv3.md"));
        await foreach (var supporter in Supporter.GetSupportersAsync())
        {
            Supporters.Add(supporter);
        }
    }

    private async void LaunchUri(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}
