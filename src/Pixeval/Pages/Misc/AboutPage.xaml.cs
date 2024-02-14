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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.AppManagement;
using WinUI3Utilities;
using Pixeval.Misc;

namespace Pixeval.Pages.Misc;

/// <summary>
/// Page that contains the information about this application, including contributor, sponsor, open source library and open source license
/// </summary>
public sealed partial class AboutPage
{
    // TODO add sponsors
    public AboutPage() => InitializeComponent();

    private async void AboutPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var licenseText = Encoding.UTF8.GetString(await AppInfo.GetAssetBytesAsync("GPLv3.md"));
        LicenseTextBlock.Text = licenseText;
    }

    private async void LaunchUri(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.GetTag<string>()));
    }

    private readonly string[] _dependencies =
    [
        "CommunityToolkit",
        "praeclarum/sqlite-net",
        // "mysticmind/reversemarkdown-net",
        // "GitTools/GitVersion",
        // "dotMorten/WinUIEx",
        "dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.Hosting",
        "codebude/QRCoder",
        "microsoft/Microsoft.IO.RecyclableMemoryStream",
        "microsoft/playwright-dotnet",
        "autofac/Autofac",
        "microsoft/Win2D",
        "Sergio0694/PolySharp",
        "SixLabors/ImageSharp",
        "reactiveui/refit"
    ];

    private IEnumerable<DependenciesViewModel> DependencyViewModels =>
        _dependencies.Select(t =>
        {
            var segments = t.Split('/');
            return new DependenciesViewModel(segments[^1], "by " + segments[0], "https://github.com/" + t);
        });

}
