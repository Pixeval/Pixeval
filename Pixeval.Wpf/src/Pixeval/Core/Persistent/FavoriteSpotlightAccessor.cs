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
using System.Linq;
using Pixeval.Data.ViewModel;
using SQLite;

namespace Pixeval.Core.Persistent
{
    public class FavoriteSpotlightAccessor : ITimelineService<FavoriteSpotlight>, IDisposable
    {
        public static FavoriteSpotlightAccessor GlobalLifeTimeScope;
        
        private readonly string path;
        private readonly SQLiteConnection sqLiteConnection;
        private readonly ObservableCollection<BrowsingHistory> delegation;

        public FavoriteSpotlightAccessor(string path)
        {
            this.path = path;
            sqLiteConnection = new SQLiteConnection(path, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            sqLiteConnection.CreateTable<FavoriteSpotlight>();
            delegation = new ObservableCollection<BrowsingHistory>(sqLiteConnection.Table<FavoriteSpotlight>().Select(FromFavoriteSpotlight));
        }

        private static BrowsingHistory FromFavoriteSpotlight(FavoriteSpotlight spotlight)
        {
            return new BrowsingHistory
            {
                BrowseObjectState = spotlight.SpotlightTitle,
                BrowseObjectId = spotlight.SpotlightArticleId,
                BrowseObjectThumbnail = spotlight.SpotlightThumbnail,
                IsReferToSpotlight = true,
                Type = "spotlight"
            };
        }

        private static FavoriteSpotlight FromBrowsingHistory(BrowsingHistory history)
        {
            return new FavoriteSpotlight
            {
                SpotlightArticleId = history.BrowseObjectId,
                SpotlightThumbnail = history.BrowseObjectThumbnail,
                SpotlightTitle = history.BrowseObjectState
            };
        }

        public Collection<BrowsingHistory> Get()
        {
            return delegation;
        }

        public void EmergencyRewrite()
        {
            if (File.Exists(path))
            {
                throw new InvalidOperationException();
            }
            using var sql = new SQLiteConnection(path);
            sql.CreateTable<FavoriteSpotlight>();
            sql.InsertAll(delegation.Select(FromBrowsingHistory));
        }

        public void Dispose()
        {
            sqLiteConnection?.Dispose();
        }

        public bool Verify(FavoriteSpotlight entity)
        {
            if (delegation.Any())
            {
                var prev = delegation[0];
                if (prev.BrowseObjectId == entity.SpotlightArticleId)
                {
                    return false;
                }
            }
            return true;
        }

        public void Insert(FavoriteSpotlight entity)
        {
            delegation.Insert(0, FromFavoriteSpotlight(entity));
        }
        
        public void Rewrite()
        {
            sqLiteConnection.DropTable<FavoriteSpotlight>();
            sqLiteConnection.CreateTable<FavoriteSpotlight>();
            sqLiteConnection.InsertAll(delegation.Select(FromBrowsingHistory));
        }

        public void DropDb()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}