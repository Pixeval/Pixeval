#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/BrowserType.cs
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
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Win32;
using WinUI3Utilities;

namespace Pixeval.Pages.Login;

public record BrowserInfo(AvailableBrowserType Type)
{
    public static readonly BrowserInfo Edge = new(AvailableBrowserType.Edge);
    public static readonly BrowserInfo Chrome = new(AvailableBrowserType.Chrome);
    public static readonly BrowserInfo Firefox = new(AvailableBrowserType.Firefox);
    public static readonly BrowserInfo WebView2 = new(AvailableBrowserType.WebView2);

    private string RegKey => @$"SOFTWARE\Clients\StartMenuInternet\{Type switch
    {
        AvailableBrowserType.Chrome => "Google Chrome",
        AvailableBrowserType.Edge => "Microsoft Edge",
        AvailableBrowserType.Firefox => "FIREFOX.EXE",
        _ => ThrowHelper.Argument<AvailableBrowserType, string>(Type)
    }}\DefaultIcon";

    public string? BrowserPath => (Registry.LocalMachine.OpenSubKey(RegKey)?.GetValue("") as string)?.Split(',')[0];

    [MemberNotNullWhen(true, nameof(BrowserPath))]
    public bool IsAvailable => Registry.LocalMachine.OpenSubKey(RegKey) is not null;

    public Uri IconPath => new(@"ms-appx:///Assets/Images/svg/" + Type switch
    {
        AvailableBrowserType.Chrome => "chrome.svg",
        AvailableBrowserType.Edge => "edge.svg",
        AvailableBrowserType.Firefox => "firefox.svg",
        AvailableBrowserType.WebView2 => "webview2.ico",
        _ => ThrowHelper.ArgumentOutOfRange<AvailableBrowserType, string>(Type)
    });

    public ImageSource IconSource => Type switch
    {
        AvailableBrowserType.Chrome or
        AvailableBrowserType.Edge or
        AvailableBrowserType.Firefox => new SvgImageSource(IconPath),
        AvailableBrowserType.WebView2 => new BitmapImage(IconPath),
        _ => ThrowHelper.ArgumentOutOfRange<AvailableBrowserType, ImageSource>(Type)
    };
}

public enum AvailableBrowserType
{
    Chrome,
    Edge,
    Firefox,
    WebView2,
    Others
}
