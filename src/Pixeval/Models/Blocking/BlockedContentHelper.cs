// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Linq;
using Mako.Model;
using Mako.Net.Responses;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Models.Blocking;

public readonly record struct BlockedContentSnapshot(
    FrozenSet<string> BlockedTags,
    FrozenSet<long> BlockedUsers);

public static class BlockedContentHelper
{
    public static BlockedContentSnapshot CaptureSnapshot()
    {
        var appViewModel = App.AppViewModel;
        var blockedTags = appViewModel.AppSettings.BrowsingExperienceSettings.BlockedTags
            .ToFrozenSet(StringComparer.Ordinal);
        var blockedUsers = appViewModel.AppServiceProvider.GetRequiredService<BlockedUserPersistentManager>()
            .GetBlockedUserIds();
        return new(blockedTags, blockedUsers);
    }

    public static bool IsBlocked(IArtworkInfo entry) => IsBlocked(entry, CaptureSnapshot());

    public static bool IsBlocked(IArtworkInfo entry, BlockedContentSnapshot snapshot) =>
        BlockedContentModelHelper.IsBlockedPlaceholder(entry)
        || entry.Tags.Any(group => group.Any(tag => snapshot.BlockedTags.Contains(tag.Name)))
        || entry.Authors.Concat(entry.Uploaders).Any(user => IsBlocked(user, snapshot));

    public static bool IsBlocked(UserBasicInfo user) => IsBlocked(user, CaptureSnapshot());

    public static bool IsBlocked(UserBasicInfo user, BlockedContentSnapshot snapshot) =>
        snapshot.BlockedUsers.Contains(user.Id);

    public static bool IsBlocked(IUser user) => IsBlocked(user, CaptureSnapshot());

    public static bool IsBlocked(IUser user, BlockedContentSnapshot snapshot) =>
        long.TryParse(user.Id, out var id) && snapshot.BlockedUsers.Contains(id);

    public static bool IsBlocked(Comment comment) => IsBlocked(comment, CaptureSnapshot());

    public static bool IsBlocked(Comment comment, BlockedContentSnapshot snapshot) =>
        snapshot.BlockedUsers.Contains(comment.User.Id);

    public static bool IsBlockedPlaceholder(IArtworkInfo entry) =>
        BlockedContentModelHelper.IsBlockedPlaceholder(entry);

    public static T Replace<T>(T entry) where T : IArtworkInfo =>
        BlockedContentModelHelper.Replace(entry, CaptureSnapshot());

    public static User Replace(User entry) =>
        BlockedContentModelHelper.Replace(entry, CaptureSnapshot());

    public static SingleUserResponse Replace(SingleUserResponse entry) =>
        BlockedContentModelHelper.Replace(entry, CaptureSnapshot());

    public static Comment Replace(Comment entry) =>
        BlockedContentModelHelper.Replace(entry, CaptureSnapshot());

    public static T ReplaceEntry<T>(T entry)
        where T : class, IIdentityInfo => ReplaceEntry(entry, CaptureSnapshot());

    public static T ReplaceEntry<T>(T entry, BlockedContentSnapshot snapshot)
        where T : class, IIdentityInfo => entry switch
    {
        IArtworkInfo artwork => (T) BlockedContentModelHelper.Replace(artwork, snapshot),
        User user => (T) (object) BlockedContentModelHelper.Replace(user, snapshot),
        Comment comment => (T) (object) BlockedContentModelHelper.Replace(comment, snapshot),
        _ => entry
    };

    public static bool TryAddOrUpdateBlockedUser(UserBasicInfo user)
    {
        if (user.Id <= 0 || App.AppViewModel?.AppServiceProvider is not { } serviceProvider)
            return false;

        var manager = serviceProvider.GetRequiredService<BlockedUserPersistentManager>();
        if (manager.GetBlockedUserIds().Contains(user.Id))
            return false;

        manager.Upsert(BlockedContentModelHelper.CreateBlockedUserEntry(user));
        return true;
    }

    public static bool TryAddOrUpdateBlockedUser(IUser user) =>
        user is UserBasicInfo userInfo && TryAddOrUpdateBlockedUser(userInfo);
}
