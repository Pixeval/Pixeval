﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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
using Pixeval.Core;
using Pixeval.Data.ViewModel;

namespace Pixeval
{
    public static class PixevalContext
    {
        public const string AppIdentifier = "Pixeval";

        public const string CurrentVersion = "3.1.4";

        public const string ConfigurationFileName = "pixeval_conf.json";

        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier.ToLower());

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string SettingsFolder = ProjectFolder;

        public static readonly string ExceptionReportFolder = Path.Combine(ProjectFolder, "crash-reports");

        public static readonly string BrowseHistoryDatabase = Path.Combine(ProjectFolder, "history.db");

        public static string PermanentlyFolder = Path.Combine(ProjectFolder, "permanent");

        public static readonly ObservableCollection<TrendingTag> TrendingTags = new ObservableCollection<TrendingTag>();

        public static readonly IQualifier<Illustration, IllustrationQualification> DefaultQualifier = new IllustrationQualifier();

        public static readonly I18NOption[] AvailableCultures = { I18NOption.UsEnglish, I18NOption.ChineseSimplified };

        public static int ProxyPort { get; set; }

        public static int PacPort { get; set; }
    }
}