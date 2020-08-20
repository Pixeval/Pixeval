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
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.Wpf.Core;
using Pixeval.Wpf.ViewModel;

namespace Pixeval.Wpf
{
    public static class AppContext
    {
        public const string AppIdentifier = "Pixeval";

        public const string CurrentVersion = "2.2.3";

        public const string ConfigurationFileName = "pixeval_conf.json";

        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier.ToLower());

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string SettingsFolder = ProjectFolder;

        public static readonly string ExceptionReportFolder = Path.Combine(ProjectFolder, "crash-reports");

        public static readonly string InterchangeFolder = Path.Combine(ProjectFolder, "interchange");

        public static readonly string CacheFolder = Path.Combine(ProjectFolder, "cache");

        public static readonly string ResourceFolder = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location)!, "Resource");

        public static readonly string PermanentlyFolder = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location)!, "Permanent");

        public static readonly string BrowseHistoryDatabase = Path.Combine(ProjectFolder, "history.db");

        public static readonly ObservableCollection<TrendingTag> TrendingTags = new ObservableCollection<TrendingTag>();

        public static readonly IQualifier<Illustration, IllustrationQualification> DefaultQualifier = new IllustrationQualifier();

        public static readonly I18NOption[] AvailableCultures = { I18NOption.USEnglish, I18NOption.MainlandChinese };

        public static int ProxyPort { get; set; }

        public static int PacPort { get; set; }

        public static async Task<bool> UpdateAvailable()
        {
            const string url = "http://47.95.218.243/Pixeval/version.txt";
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url) != CurrentVersion;
        }
    }
}
