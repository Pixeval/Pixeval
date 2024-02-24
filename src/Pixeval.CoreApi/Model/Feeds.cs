#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/Feeds.cs
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

public record Feed
{
    /// <summary>
    /// May points to user, illustration or novel
    /// </summary>
    public string? FeedId { get; set; }

    /// <summary>
    /// The name of the target of this feed if it has one
    /// e.g. illust title or the user name
    /// </summary>
    public string? FeedName { get; set; }

    /// <summary>
    /// The thumbnail of the target of this feed if it has one
    /// </summary>
    public string? FeedThumbnail { get; set; }

    /// <summary>
    /// The user who posts this feed
    /// </summary>
    public string? PostUserId { get; set; }

    public string? PostUserName { get; set; }

    /// <summary>
    /// The creator's name of the illustration/novel if possible
    /// </summary>
    public string? ArtistName { get; set; }

    public DateTimeOffset PostDate { get; set; }

    public FeedType? Type { get; set; }

    /// <summary>
    /// The thumbnail of the user who posts this feed
    /// </summary>
    public string? PostUserThumbnail { get; set; }

    /// <summary>
    /// Is this feed's target pointing to an user
    /// </summary>
    public bool IsTargetRefersToUser { get; set; }
}

public enum FeedType
{
    /// <summary>
    /// User added a new bookmark
    /// </summary>
    AddBookmark,

    /// <summary>
    /// User posted a new illust
    /// </summary>
    AddIllust,

    /// <summary>
    /// User followed an artist
    /// </summary>
    AddFavorite,

    /// <summary>
    /// User added a new novel to bookmarks
    /// </summary>
    AddNovelBookmark
}