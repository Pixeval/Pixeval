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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Data.ViewModel;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class DownloadManager
    {
        public static readonly ObservableCollection<DownloadableIllustration> Downloading = new ObservableCollection<DownloadableIllustration>();

        public static readonly ObservableCollection<DownloadableIllustration> Downloaded = new ObservableCollection<DownloadableIllustration>();

        public static void EnqueueDownloadItem(Illustration illustration, string path = null)
        {
            if (Downloading.Any(i => illustration.Id == i.DownloadContent.Id))
            {
                return;
            }
            
            static DownloadableIllustration CreateDownloadableIllustration(Illustration downloadContent, bool isFromMange, string downloadPath)
            {
                var model = new DownloadableIllustration(downloadContent, downloadPath ?? PixivHelper.FormatDownloadPath(Settings.Global.DownloadPath, downloadContent), isFromMange);
                model.State.ValueChanged += (sender, args) => Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (args.NewValue)
                    {
                        case DownloadState.Finished:
                            model.Freeze();
                            Downloading.Remove(model);
                            if (Downloaded.All(i => model.DownloadContent.GetDownloadUrl() != i.DownloadContent.GetDownloadUrl()))
                                Downloaded.Add(model);
                            break;
                        case DownloadState.Downloading:
                            Downloaded.Remove(model);
                            Downloading.Add(model);
                            break;
                        case DownloadState.Canceled:
                            Downloading.Remove(model);
                            break;
                        case DownloadState.Queue:
                        case DownloadState.Exceptional:
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                });
                return model;
            }

            if (illustration.IsManga)
            {
                foreach (var t in illustration.MangaMetadata)
                {
                    Task.Run(() => CreateDownloadableIllustration(t, true, path).Download());
                }
            }
            else
            {
                Task.Run(() => CreateDownloadableIllustration(illustration, false, path).Download());
            }
        }
    }

    public class SearchingHistoryManager
    {
        private static readonly ObservableCollection<string> SearchingHistory = new ObservableCollection<string>();

        public static void EnqueueSearchHistory(string keyword)
        {
            if (SearchingHistory.Count == 4)
            {
                SearchingHistory.RemoveAt(SearchingHistory.Count - 1);
            }
            SearchingHistory.Insert(0, keyword);
        }

        public static IEnumerable<string> GetSearchingHistory()
        {
            return SearchingHistory;
        }
    }
}