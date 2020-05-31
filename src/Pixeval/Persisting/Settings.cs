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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pixeval.Core;
using Pixeval.Objects.Caching;
using Pixeval.Objects.Primitive;
using PropertyChanged;

namespace Pixeval.Persisting
{
    /// <summary>
    ///     A class represents user preference
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class Settings
    {
        public static Settings Global = new Settings();

        private string downloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        /// <summary>
        ///     Insert illustration to the sorted list, order by it's Bookmark property
        /// </summary>
        public bool SortOnInserting { get; set; }

        /// <summary>
        ///     The minimum bookmark to be filter
        /// </summary>
        public int MinBookmark { get; set; } = 1;

        /// <summary>
        ///     Determines whether Pixeval should recommend illustrators or not
        /// </summary>
        public bool RecommendIllustrator { get; set; }

        /// <summary>
        ///     The default download location
        /// </summary>
        public string DownloadLocation
        {
            get => downloadLocation.IsNullOrEmpty()
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                : downloadLocation;
            set => downloadLocation = value;
        }

        /// <summary>
        ///     Determines whether Pixeval should use cache
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        ///     Set the caching policy of Pixeval, accept values are Memory and File
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CachingPolicy CachingPolicy { get; set; } = CachingPolicy.Memory;

        /// <summary>
        ///     Indicate the way to match tags
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SearchTagMatchOption TagMatchOption { get; set; } = SearchTagMatchOption.PartialMatchForTags;

        /// <summary>
        ///     How many pages need to be queried once, minimum is 1, maximum is 10
        /// </summary>
        public int QueryPages { get; set; } = 1;

        /// <summary>
        ///     The page number of the query start
        /// </summary>
        public int QueryStart { get; set; } = 1;

        /// <summary>
        ///     Determines whether Pixeval should use direct connect(bypass SNI)
        /// </summary>
        public bool DirectConnect { get; set; }

        /// <summary>
        ///     The spotlight page number of the query start
        /// </summary>
        public int SpotlightQueryStart { get; set; } = 1;

        /// <summary>
        ///     Tags to be exclude
        /// </summary>
        public ISet<string> ExcludeTag { get; set; } = new HashSet<string>();

        /// <summary>
        ///     Tags to be include
        /// </summary>
        public ISet<string> IncludeTag { get; set; } = new HashSet<string>();

        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        ///     Save current settings to local
        /// </summary>
        /// <returns></returns>
        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(AppContext.SettingsFolder, "settings.json"), Global.ToString());
        }

        /// <summary>
        ///     Load settings from local
        /// </summary>
        /// <returns></returns>
        public static async Task Restore()
        {
            if (File.Exists(Path.Combine(AppContext.SettingsFolder, "settings.json")))
                Global = (await File.ReadAllTextAsync(Path.Combine(AppContext.SettingsFolder, "settings.json")))
                    .FromJson<Settings>();
            else
                Initialize();
        }

        /// <summary>
        ///     Initialize the default value of settings
        /// </summary>
        public static void Initialize()
        {
            if (File.Exists(Path.Combine(AppContext.SettingsFolder, "settings.json"))) File.Delete(Path.Combine(AppContext.SettingsFolder, "settings.json"));

            Global.downloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Global.DownloadLocation = string.Empty;
            Global.SortOnInserting = false;
            Global.IncludeTag = new HashSet<string>();
            Global.ExcludeTag = new HashSet<string>();
            Global.DirectConnect = false;
            Global.MinBookmark = 0;
            Global.QueryPages = 1;
            Global.QueryStart = 1;
            Global.SpotlightQueryStart = 1;
            Global.RecommendIllustrator = false;
            Global.UseCache = false;
            Global.CachingPolicy = CachingPolicy.Memory;
            Global.TagMatchOption = SearchTagMatchOption.PartialMatchForTags;
        }
    }
}