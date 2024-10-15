#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/IIllustrate.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;

namespace Pixeval.CoreApi.Model;

public interface IEntry;

public interface IIdEntry : IEntry
{
    long Id { get; }
}

public interface IWorkEntry : IIdEntry
{
    int TotalView { get; }

    int TotalBookmarks { get; }

    bool IsBookmarked { get; set; }

    bool IsPrivate { get; set; }

    bool IsMuted { get; set; }

    Tag[] Tags { get; }

    string Title { get; }

    string Caption { get; }

    UserInfo User { get; }

    DateTimeOffset CreateDate { get; }

    ImageUrls ThumbnailUrls { get; }

    /// <summary>
    /// 值为2是AI生成
    /// </summary>
    int AiType { get; }

    XRestrict XRestrict { get; }
}
