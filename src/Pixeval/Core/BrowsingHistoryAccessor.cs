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
using System.IO;
using System.Linq;
using Pixeval.Data.ViewModel;
using SQLite;

namespace Pixeval.Core
{
    /// <summary>
    ///     Manages the global browsing history which lives inside application
    ///     the underlying implementation is sqlite database
    /// </summary>
    public class BrowsingHistoryAccessor : ITimelineService, IDisposable
    {
        public static BrowsingHistoryAccessor GlobalLifeTimeScope;

        // current browsing histories
        private readonly ObservableCollection<BrowsingHistory> _delegation;
        private readonly string _path;
        private readonly SQLiteConnection _sqLiteConnection;
        private readonly int _stackLimit;
        private bool _writable;

        public BrowsingHistoryAccessor(int stackLimit, string path)
        {
            _stackLimit = stackLimit;
            _path = path;
            _sqLiteConnection = new SQLiteConnection(path, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            _sqLiteConnection.CreateTable<BrowsingHistory>();
            _delegation = new ObservableCollection<BrowsingHistory>(_sqLiteConnection.Table<BrowsingHistory>());
        }

        public void Dispose()
        {
            _sqLiteConnection?.Dispose();
        }

        public bool VerifyRationality(BrowsingHistory browsingHistory)
        {
            // if current browsing history list has elements
            if (_delegation.Any())
            {
                var prev = _delegation[0];
                // check if the last one in the browsing history list is the same as the one to be insert, return false if so
                if (prev.Type == browsingHistory.Type && prev.BrowseObjectId == browsingHistory.BrowseObjectId) return false;
            }

            return true;
        }

        public void Insert(BrowsingHistory browsingHistory)
        {
            // we can simply consider the browsing histories as a double-ended queue with limited capacity, we will pop the oldest one
            // and insert a new one if the Deque is full
            if (_delegation.Count >= _stackLimit) _delegation.Remove(_delegation.Last());

            _delegation.Insert(0, browsingHistory);
        }

        /// <summary>
        ///     If the database file is not present, we will urgently create one and
        ///     write all the browsing histories that we have into it
        /// </summary>
        public void EmergencyRewrite()
        {
            if (File.Exists(_path)) throw new InvalidOperationException();
            using var sql = new SQLiteConnection(_path);
            sql.CreateTable<BrowsingHistory>();
            sql.InsertAll(Get());
        }

        /// <summary>
        ///     Returns currently maintained browsing histories
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BrowsingHistory> Get()
        {
            return _delegation;
        }

        /// <summary>
        ///     Rewrite the local database, you muse call <see cref="SetWritable" />
        ///     before call this method
        /// </summary>
        public void Rewrite()
        {
            if (!_writable) throw new InvalidOperationException();
            _sqLiteConnection.DropTable<BrowsingHistory>();
            _sqLiteConnection.CreateTable<BrowsingHistory>();
            _sqLiteConnection.InsertAll(_delegation);
        }

        /// <summary>
        ///     Delete the local database
        /// </summary>
        public void DropDb()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }

        public void SetWritable()
        {
            _writable = true;
        }
    }
}