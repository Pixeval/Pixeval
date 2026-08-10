// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Controls;
using Pixeval.Models.Blocking;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public partial class UserItemViewModel
{
    [RelayCommand]
    private async Task FollowAsync()
    {
        if ((IsFollowedDisplay & HeartButtonState.Pending) is not 0)
            return;
        IsFollowedDisplay |= HeartButtonState.Pending; // pre-update
        var result = await MakoHelper.SetFollowAsync(Entry, !IsFollowed);
        IsFollowedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
    }

    private bool CanBlockUser => !BlockedContentHelper.IsBlocked(Entry.UserInfo);

    [RelayCommand(CanExecute = nameof(CanBlockUser))]
    private void BlockUser()
    {
        if (BlockedContentHelper.TryAddOrUpdateBlockedUser(Entry.UserInfo))
            BlockUserCommand.NotifyCanExecuteChanged();
    }

    public override Uri AppUri => Entry.UserInfo.AppUri;

    public override Uri WebsiteUri => Entry.UserInfo.WebsiteUri;
}
