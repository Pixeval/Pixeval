// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Mako.Model;
using Mako.Net.Responses;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Blocking;
using Pixeval.Models.Database;

namespace Pixeval.Utilities;

public static class BlockedContentModelHelper
{
    // Mako entries do not expose extension state, so keep the placeholder marker outside the model.
    private static readonly ConditionalWeakTable<IArtworkInfo, object> _BlockedArtworkMarkers = new();

    private static readonly object _BlockedArtworkMarker = new();

    public static BlockedUserEntry CreateBlockedUserEntry(UserBasicInfo user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Account = user.Account,
        AvatarUrl = user.AvatarUrl
    };

    public static UserBasicInfo CreateBlockedUserPreview(BlockedUserEntry entry) => new BlockedUserPreview(entry);

    public static NovelContent CreateBlockedNovelContent(Novel entry) => NovelContent.CreateDefault() with
    {
        Id = entry.Id,
        Title = entry.Title,
        UserId = entry.User.Id,
        CoverUrl = AppInfo.BlockedContentPath,
        Date = entry.CreateDate,
        Text = I18NManager.GetResource(BlockedContentResources.Work)
    };

    internal static bool IsBlockedPlaceholder(IArtworkInfo entry) =>
        _BlockedArtworkMarkers.TryGetValue(entry, out _);

    internal static IArtworkInfo Replace(IArtworkInfo entry, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(entry, snapshot)
            ? entry switch
            {
                Illustration illustration => Replace(illustration, snapshot),
                Novel novel => Replace(novel, snapshot),
                _ => entry
            }
            : entry;

    internal static Illustration Replace(Illustration entry, BlockedContentSnapshot snapshot) =>
        MarkBlocked(Illustration.CreateDefault() with
        {
            Id = entry.Id,
            Title = I18NManager.GetResource(BlockedContentResources.Work),
            User = Replace(entry.User, snapshot),
            CreateDate = entry.CreateDate,
            ThumbnailUrls = CreatePlaceholderImageUrls(),
            MetaSinglePage = new() { OriginalImageUrl = AppInfo.BlockedContentPath },
            PageCount = 1,
            Width = 1,
            Height = 1
        });

    internal static Novel Replace(Novel entry, BlockedContentSnapshot snapshot) =>
        MarkBlocked(Novel.CreateDefault() with
        {
            Id = entry.Id,
            Title = I18NManager.GetResource(BlockedContentResources.Work),
            User = Replace(entry.User, snapshot),
            CreateDate = entry.CreateDate,
            ThumbnailUrls = CreatePlaceholderImageUrls(),
            PageCount = 1
        });

    internal static User Replace(User entry, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(entry.UserInfo, snapshot)
            ? User.CreateDefault() with
            {
                UserInfo = Replace(entry.UserInfo, snapshot),
                IsMuted = true
            }
            : entry;

    internal static SingleUserResponse Replace(SingleUserResponse entry, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(entry.UserEntity, snapshot)
            ? SingleUserResponse.CreateDefault() with
            {
                UserEntity = Replace(entry.UserEntity, snapshot),
                UserProfile = Profile.CreateDefault() with { BackgroundImageUrl = AppInfo.BlockedContentPath }
            }
            : entry;

    internal static Comment Replace(Comment entry, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(entry, snapshot)
            ? Comment.CreateDefault() with
            {
                Id = entry.Id,
                Content = I18NManager.GetResource(BlockedContentResources.Comment),
                Date = entry.Date,
                User = Replace(entry.User, snapshot)
            }
            : entry;

    private static UserInfo Replace(UserInfo user, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(user, snapshot)
            ? UserInfo.CreateDefault() with
            {
                Id = user.Id,
                Name = I18NManager.GetResource(BlockedContentResources.User),
                Account = user.Account,
                ProfileImageUrls = CreatePlaceholderAvatarUrl()
            }
            : user;

    private static SingleUserInfo Replace(SingleUserInfo user, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(user, snapshot)
            ? SingleUserInfo.CreateDefault() with
            {
                Id = user.Id,
                Name = I18NManager.GetResource(BlockedContentResources.User),
                Account = user.Account,
                ProfileImageUrls = CreatePlaceholderAvatarUrl(),
                Description = I18NManager.GetResource(BlockedContentResources.User)
            }
            : user;

    private static AvatarUser Replace(AvatarUser user, BlockedContentSnapshot snapshot) =>
        BlockedContentHelper.IsBlocked(user, snapshot)
            ? AvatarUser.CreateDefault() with
            {
                Id = user.Id,
                Name = I18NManager.GetResource(BlockedContentResources.User),
                Account = user.Account,
                ProfileImageUrls = CreatePlaceholderAvatarUrl()
            }
            : user;

    private static ImageUrls CreatePlaceholderImageUrls() => new()
    {
        SquareMedium = AppInfo.BlockedContentPath,
        Medium = AppInfo.BlockedContentPath,
        Large = AppInfo.BlockedContentPath
    };

    private static MediumOnlyImageUrl CreatePlaceholderAvatarUrl() => new() { Medium = AppInfo.BlockedContentPath };

    private static T MarkBlocked<T>(T entry)
        where T : class, IArtworkInfo
    {
        if (!_BlockedArtworkMarkers.TryGetValue(entry, out _))
            _BlockedArtworkMarkers.Add(entry, _BlockedArtworkMarker);
        return entry;
    }

    private sealed record BlockedUserPreview : UserBasicInfo
    {
        [SetsRequiredMembers]
        public BlockedUserPreview(BlockedUserEntry entry)
        {
            Id = entry.Id;
            Name = entry.DisplayName;
            Account = entry.Account;
            AvatarUrl = string.IsNullOrWhiteSpace(entry.AvatarUrl)
                ? AppInfo.BlockedContentPath
                : entry.AvatarUrl;
        }

        public override string AvatarUrl { get; }
    }
}
