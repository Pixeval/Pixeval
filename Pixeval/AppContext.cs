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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pixeval.Interfaces;
using Pixeval.Models;
using Pixeval.Types;

namespace Pixeval
{
    public static class AppContext
    {
        public const string AppIdentifier = "Pixeval";

        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier.ToLower());

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string SettingsFolder = ProjectFolder;

        public static readonly string ExceptionReportFolder = Path.Combine(ProjectFolder, "crash-reports");

        public static readonly string ConfigurationFileName = "pixeval_conf.json";

        public static readonly IConfiguration Configuration;

        internal static bool LogoutExit = false;

        public static IDownloadPathProvider DownloadPathProvider = new DefaultDownloadPathProvider();

        public static IIllustrationFileNameFormatter FileNameFormatter = new DefaultIllustrationFileNameFormatter();

        public static ObservableCollection<DownloadableIllustrationViewModel> Downloading = new ObservableCollection<DownloadableIllustrationViewModel>();

        public static readonly ObservableCollection<TrendingTag> TrendingTags = new ObservableCollection<TrendingTag>();

        private static readonly ObservableCollection<string> SearchingHistory = new ObservableCollection<string>();

        static AppContext()
        {
            Directory.CreateDirectory(ProjectFolder);
            Directory.CreateDirectory(SettingsFolder);
            Directory.CreateDirectory(ExceptionReportFolder);
            Configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(SettingsFolder, ConfigurationFileName))
                .Build();
        }

        public static void EnqueueSearchHistory(string keyword)
        {
            if (SearchingHistory.Count == 4) SearchingHistory.RemoveAt(SearchingHistory.Count - 1);
            SearchingHistory.Insert(0, keyword);
        }

        public static IReadOnlyCollection<string> GetSearchingHistory()
        {
            return SearchingHistory;
        }

        public static void EnqueueDownloadItem(Illustration illustration)
        {
            static void RemoveAction(DownloadableIllustrationViewModel d)
            {
                Downloading.Remove(d);
            }

            if (illustration.IsManga)
            {
                for (var j = 0; j < illustration.MangaMetadata.Length; j++)
                {
                    var model = new DownloadableIllustrationViewModel(illustration.MangaMetadata[j], true, j) {DownloadFinished = RemoveAction};
                    Task.Run(() => model.Download());
                    Downloading.Add(model);
                }
            }
            else
            {
                var model = new DownloadableIllustrationViewModel(illustration, false) {DownloadFinished = RemoveAction};
                Task.Run(() => model.Download());
                Downloading.Add(model);
            }
        }
    }
}