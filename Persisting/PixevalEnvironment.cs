// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.IO;

namespace Pixeval.Persisting
{
    public class PixevalEnvironment
    {
        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pixeval");

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string TempFolder = Path.Combine(ProjectFolder, "tmp");

        public static readonly string SettingsFolder = ProjectFolder;

        internal static bool LogoutExit = false;

        static PixevalEnvironment()
        {
            if (!Directory.Exists(ProjectFolder)) Directory.CreateDirectory(ProjectFolder);

            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);

            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
        }
    }
}