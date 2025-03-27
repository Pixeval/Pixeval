// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;

namespace Mako.Model;

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
    AiType AiType { get; }

    XRestrict XRestrict { get; }
}
