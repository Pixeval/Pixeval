// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Text;
using Windows.System;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls;
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
        UniformGrid.SizeChanged += (sender, args) => sender.To<UniformGrid>().Columns = (int)(args.NewSize.Width / 140);
    }

    // private readonly ObservableCollection<Supporter> _supporters = [];

    private async void AboutPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        LicenseTextBlock.Text = Encoding.UTF8.GetString(await AppInfo.GetAssetBytesAsync("GPLv3.md"));
        await foreach (var supporter in Supporter.GetSupportersAsync())
        {
            UniformGrid.Children.Add(new PersonView
            {
                PersonName = supporter.AtName,
                PersonNickname = supporter.Nickname,
                PersonPicture = supporter.ProfileImage,
                PersonProfileNavigateUri = supporter.ProfileUri,
                Height = 160
            });
        }
        //     _supporters.Add(supporter);
    }

    private async void LaunchUri(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}
