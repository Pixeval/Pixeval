// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Mako.Model;
using Misaki;
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
            return field;
        }
    }

    public static IllustrationItemViewModel DesignIllustrationViewModel => field ??= new(DesignIllustration);

    public static NovelItemViewModel DesignNovelViewModel => field ??= new(DesignNovel);

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

    public static ISingleImage DownloadParserSampleWork(ImageType imageType) => new DownloadParserSampleWork(imageType);
}

file record DownloadParserSampleWork(ImageType ImageType) : ISingleImage, IImageSet, ISingleAnimatedImage
{
    public ulong ByteSize => 0;

    public Uri ImageUri => null!;

    public int SetIndex => 0;

    public IPreloadableList<ISingleImage> Pages => null!;

    public int PageCount => 0;

    public SingleAnimatedImageType PreferredAnimatedImageType => SingleAnimatedImageType.SingleZipFile;

    public Uri? SingleImageUri => null;

    public IPreloadableList<int>? ZipImageDelays => null;

    public IPreloadableList<(Uri Uri, int MsDelay)>? MultiImageUris => null;

    public IPreloadableList<IAnimatedImageFrame> AnimatedThumbnails => null!;

    public int Width => 0;

    public int Height => 0;

    public string Platform => null!;

    public string Id => "12345678";

    public string Title => nameof(Title);

    public string Description => null!;

    public Uri WebsiteUri => null!;

    public Uri AppUri => null!;

    public DateTimeOffset CreateDate => new(2020, 10, 12, 0, 0, 0, TimeSpan.Zero);

    public IPreloadableList<IUser> Authors { get; } =
    [
        new UserInfo
        {
            Id = 7654321,
            Name = nameof(UserInfo.Name),
            Account = "",
            ProfileImageUrls = null!
        }
    ];

    public IPreloadableList<IUser> Uploaders => null!;

    public SafeRating SafeRating => default;

    public ILookup<ITagCategory, ITag> Tags => null!;

    public IReadOnlyCollection<IImageFrame> Thumbnails => null!;

    public IReadOnlyDictionary<string, object> AdditionalInfo => null!;

    public int TotalFavorite => 0;

    public int TotalView => 0;

    public bool IsFavorite => false;

    public bool IsAiGenerated => false;
}
