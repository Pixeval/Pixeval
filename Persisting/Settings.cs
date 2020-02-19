// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

        private string downloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public bool SortOnInserting { get; set; }

        public int MinBookmark { get; set; } = 1;

        public bool RecommendIllustrator { get; set; }

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
            if (File.Exists(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json")))
                Global = (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"))).FromJson<Settings>();
            else
                Initialize();
            return Global;
        }

        public void Initialize()
        {
            if (File.Exists(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json")))
                File.Delete(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"));

            Global.downloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Global.DownloadLocation = string.Empty;
            Global.SortOnInserting = false;
            Global.ContainsTags = new HashSet<string>();
            Global.ExceptTags = new HashSet<string>();
            Global.MinBookmark = 0;
            Global.QueryPages = 1;
            Global.QueryStart = 1;
            Global.SpotlightQueryStart = 1;
            Global.RecommendIllustrator = false;
        }
    }
}