// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using Mako.Net.Responses;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalUserDto(
    long Id,
    string Name,
    string Account,
    string AvatarUrl,
    string Description,
    bool? IsFollowed,
    string WebsiteUrl,
    string PixevalUri,
    PixevalUserProfileDto? Profile = null)
{
    public static PixevalUserDto FromUserInfo(UserBasicInfo user) =>
        new(
            user.Id,
            user.Name,
            user.Account,
            user.AvatarUrl,
            user.Description,
            (user as UserInfo)?.IsFollowed,
            user.WebsiteUri.ToString(),
            user.AppUri.ToString());

    public static PixevalUserDto FromUser(User user) => FromUserInfo(user.UserInfo);

    public static PixevalUserDto FromSingleUserResponse(SingleUserResponse user)
    {
        var profile = user.UserProfile;
        return FromUserInfo(user.UserEntity) with
        {
            Profile = new(
                profile.TotalFollowUsers,
                profile.TotalIllustrations,
                profile.TotalManga,
                profile.TotalNovels,
                profile.TotalIllustrationBookmarksPublic,
                profile.Webpage,
                profile.TwitterUrl,
                profile.BackgroundImageUrl)
        };
    }
}