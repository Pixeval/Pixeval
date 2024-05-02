#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AboutPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Net.Http;
using System.Text;
using Windows.System;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
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

    private async void AboutPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        LicenseTextBlock.Text = Encoding.UTF8.GetString(await AppInfo.GetAssetBytesAsync("GPLv3.md"));

        await foreach (var supporter in Supporter.GetSupportersAsync(new HttpClient()))
        {
            UniformGrid.Children.Add(new PersonView
            {
                PersonName = supporter.Name,
                PersonNickname = supporter.Nickname,
                PersonPicture = supporter.ProfilePicture,
                PersonProfileNavigateUri = supporter.ProfileUri,
                Height = 160
            });
        }
    }

    private async void LaunchUri(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}
