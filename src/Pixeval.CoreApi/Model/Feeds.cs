#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Feeds.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record Feed
    {
        /// <summary>
        ///     May points to user, illustration or novel
        /// </summary>
        public string? FeedId { get; set; }

        /// <summary>
        ///     The name of the target of this feed if it has one
        ///     e.g. illust title or the user name
        /// </summary>
        public string? FeedName { get; set; }

        /// <summary>
        ///     The thumbnail of the target of this feed if it has one
        /// </summary>
        public string? FeedThumbnail { get; set; }

        /// <summary>
        ///     The user who posts this feed
        /// </summary>
        public string? PostUserId { get; set; }

        public string? PostUserName { get; set; }

        /// <summary>
        ///     The creator's name of the illustration/novel if possible
        /// </summary>
        public string? ArtistName { get; set; }

        public DateTimeOffset PostDate { get; set; }

        public FeedType? Type { get; set; }

        /// <summary>
        ///     The thumbnail of the user who posts this feed
        /// </summary>
        public string? PostUserThumbnail { get; set; }

        /// <summary>
        ///     Is this feed's target pointing to an user
        /// </summary>
        public bool IsTargetRefersToUser { get; set; }
    }

    public enum FeedType
    {
        /// <summary>
        ///     User added a new bookmark
        /// </summary>
        AddBookmark,

        /// <summary>
        ///     User posted a new illust
        /// </summary>
        AddIllust,

        /// <summary>
        ///     User followed an artist
        /// </summary>
        AddFavorite,

        /// <summary>
        ///     User added a new novel to bookmarks
        /// </summary>
        AddNovelBookmark
    }
}