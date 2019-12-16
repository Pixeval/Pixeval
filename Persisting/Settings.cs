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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Objects;
using PropertyChanged;

namespace Pixeval.Persisting
{
    [AddINotifyPropertyChangedInterface]
    public class Settings
    {
        public static Settings Global = new Settings();

        private string downloadLocation;

        public bool SyncOnStart { get; set; }

        public bool SortOnInserting { get; set; }

        public bool CachingThumbnail { get; set; }

        public int MinBookmark { get; set; }

        public string DownloadLocation
        {
            get => downloadLocation.IsNullOrEmpty() ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : downloadLocation;
            set => downloadLocation = value;
        }

        public int QueryPages { get; set; } = 1;

        public int QueryStart { get; set; } = 1;

        public int SpotlightQueryStart { get; set; } = 1;

        public ISet<string> ExceptTags { get; set; } = new HashSet<string>();

        public ISet<string> ContainsTags { get; set; } = new HashSet<string>();

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"), Global.ToString());
        }

        public async Task<Settings> Restore()
        {
            if (File.Exists(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"))) Global = (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"))).FromJson<Settings>();
            return Global;
        }

        public void Clear()
        {
            File.Delete(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"));
            Global.DownloadLocation = null;
            Global.SortOnInserting = false;
            Global.CachingThumbnail = false;
            Global.ContainsTags = null;
            Global.ExceptTags = null;
            Global.SyncOnStart = false;
            Global.MinBookmark = 0;
            Global.QueryPages = 1;
            Global.QueryStart = 1;
            Global.SpotlightQueryStart = 1;
        }
    }
}