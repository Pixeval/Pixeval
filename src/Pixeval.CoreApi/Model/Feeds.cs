// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.CoreApi.Model;

public record Feed : IIdEntry
{
    /// <summary>
    /// May points to user, illustration or novel
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The name of the target of this feed if it has one
    /// e.g. illust title or the username
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

    public string? PostUsername { get; set; }

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
    /// Is this feed's target pointing to a user
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
    PostIllust,

    /// <summary>
    /// User followed an artist
    /// </summary>
    AddFavorite,

    /// <summary>
    /// User added a new novel to bookmarks
    /// </summary>
    AddNovelBookmark
}
