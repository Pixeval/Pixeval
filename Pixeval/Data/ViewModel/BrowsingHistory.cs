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

using PropertyChanged;
using SQLite;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class BrowsingHistory
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Index { get; set; }

        /// <summary>
        ///     提供当前<see cref="BrowsingHistory" />的<see cref="string" />视图，默认的值是名称/标题
        /// </summary>
        public string BrowseObjectState { get; set; }

        public string BrowseObjectThumbnail { get; set; }

        /// <summary>
        ///     <see cref="IsReferToIllust" />有效仅当此属性有效
        /// </summary>
        public string IllustratorName { get; set; }

        public bool IsReferToUser { get; set; }

        public bool IsReferToIllust { get; set; }

        public bool IsReferToSpotlight { get; set; }

        /// <summary>
        ///     提供当前<see cref="BrowsingHistory" />的id的<see cref="string" />视图，默认的值是作品ID/用户ID/特辑ID
        /// </summary>
        public string BrowseObjectId { get; set; }

        public string Type { get; set; }
    }
}