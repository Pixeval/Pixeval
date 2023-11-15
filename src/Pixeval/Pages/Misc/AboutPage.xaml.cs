#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/AboutPage.xaml.cs
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
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Misc;

/// <summary>
/// Page that contains the information about this application, including contributor, sponsor, open source library and open source license
/// </summary>
public sealed partial class AboutPage
{
    // TODO add sponsors
    public AboutPage()
    {
        InitializeComponent();
    }

    private string Test => "/AboutPage/QRCoderIntroductoryTextBlock";

    private async void AboutPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var licenseText = Encoding.UTF8.GetString(await AppContext.GetAssetBytesAsync("GPLv3.md"));
        LicenseTextBlock.Text = licenseText;
    }

    private async void LaunchUri(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.GetTag<string>()));
    }
}
