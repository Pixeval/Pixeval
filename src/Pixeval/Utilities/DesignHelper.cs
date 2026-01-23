// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.ViewModels;

namespace Pixeval.Utilities;

public static class DesignHelper
{
    public static UserItemViewModel DesignUserViewModel
    {
        get
        {
            field ??= new(DesignUser);
            _ = field.LoadAvatarAsync();
            return field;
        }
    }

    public static IllustrationItemViewModel DesignIllustrationViewModel
    {
        get
        {
            field ??= new(DesignIllustration);
            _ = field.TryLoadThumbnailAsync(0);
            return field;
        }
    }

    public static NovelItemViewModel DesignNovelViewModel
    {
        get
        {
            field ??= new(DesignNovel);
            _ = field.TryLoadThumbnailAsync(0);
            return field;
        }
    }

    public static User DesignUser => field ??= new()
    {
        UserInfo = DesignUserInfo,
        Illustrations = [DesignIllustration, DesignIllustration, DesignIllustration],
        Novels = [DesignNovel, DesignNovel, DesignNovel],
        IsMuted = false
    };

    public static Novel DesignNovel => field ??= new()
    {
        Id = 123456,
        Title = "Title",
        Description = "Description",
        IsPrivate = false,
        XRestrict = XRestrict.R18,
        User = DesignUserInfo,
        CreateDate = DateTimeOffset.UtcNow,
        ThumbnailUrls = new()
        {
            Large = AppInfo.ImageNotAvailablePath,
            SquareMedium = AppInfo.ImageNotAvailablePath,
            Medium = AppInfo.ImageNotAvailablePath
        },
        IsFavorite = false,
        TotalFavorite = 123,
        TotalView = 456,
        Visible = true,
        IsMuted = false,
        IsOriginal = false,
        PageCount = 3,
        TextLength = 50,
        IsMypixivOnly = false,
        IsXRestricted = true,
        TotalComments = 3,
        AiType = AiType.NotSpecified,
        Series = null,
        Tags =
        [
            new() { Name = "Tag A", TranslatedName = null },
            new() { Name = "Tag B", TranslatedName = null },
            new() { Name = "Tag C", TranslatedName = null }
        ]
    };

    public static Illustration DesignIllustration => field ??= new()
    {
        Type = IllustrationType.Illust,
        Tools = [],
        PageCount = 3,
        Width = 800,
        Height = 600,
        SanityLevel = 0,
        MetaSinglePage = new()
        {
            OriginalImageUrl = AppInfo.ImageNotAvailablePath
        },
        MetaPages = [],
        AiType = AiType.AiGenerated,
        IllustrationBookStyle = 0,
        Id = 123456,
        Title = "Title",
        Description = "Description",
        IsPrivate = false,
        XRestrict = XRestrict.R18,
        User = DesignUserInfo,
        CreateDate = DateTimeOffset.UtcNow,
        ThumbnailUrls = new()
        {
            Large = AppInfo.ImageNotAvailablePath,
            SquareMedium = AppInfo.ImageNotAvailablePath,
            Medium = AppInfo.ImageNotAvailablePath
        },
        IsFavorite = true,
        TotalFavorite = 123,
        TotalView = 456,
        Visible = true,
        IsMuted = false,
        Series = null,
        Tags =
        [
            new() { Name = "Tag A", TranslatedName = null },
            new() { Name = "Tag B", TranslatedName = null },
            new() { Name = "Tag C", TranslatedName = null }
        ]
    };

    public static UserInfo DesignUserInfo => field ??= new()
    {
        Id = 123456,
        Name = "Username",
        Account = "Account",
        IsFollowed = true,
        ProfileImageUrls = new()
        {
            Medium = AppInfo.PixivNoProfilePath
        }
    };
}
