#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Pixeval.Objects.Primitive;

namespace Pixeval.Objects.Native
{
    public class WallAdjudicator
    {
        private const int SetDeskWallpaper = 20;
        private const int UpdateIniFile = 0x01;
        private const int SendWinIniChange = 0x02;

        public WallAdjudicator(string storeLocation, BitmapSource background)
        {
            StoreLocation = storeLocation;
            Background = background;
        }

        public string StoreLocation { get; set; }

        public BitmapSource Background { get; set; }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParameter, string lpvParameter, int fuWinIni);

        private async Task CreateBmpFile()
        {
            await Background.Save<BmpBitmapEncoder>(StoreLocation);
        }

        private static bool CheckOperatingSystemVersion()
        {
            var win8Version = new Version(6, 2, 9200, 0);
            return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= win8Version;
        }

        private bool IsImageSupportSpan()
        {
            return Background.PixelWidth / (double) Background.PixelHeight >= 1.7;
        }

        public async ValueTask<bool> Execute()
        {
            await CreateBmpFile();
            using var regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (regKey == null) return false;
            regKey.SetValue("WallpaperStyle",
                            (CheckOperatingSystemVersion() && IsImageSupportSpan() ? /* span */ 22 : /* normal */ 10).ToString());
            regKey.SetValue("TitleWallpaper", 0.ToString());
            SystemParametersInfo(SetDeskWallpaper, 0, StoreLocation, UpdateIniFile | SendWinIniChange);
            return true;
        }
    }
}
