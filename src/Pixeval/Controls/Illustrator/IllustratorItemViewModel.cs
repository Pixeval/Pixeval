// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public sealed partial class IllustratorItemViewModel : EntryViewModel<User>, IFactory<User, IllustratorItemViewModel>
{
    public static IllustratorItemViewModel CreateInstance(User entry) => new(entry);

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    public IllustratorItemViewModel(User user) : base(user)
    {
        IsFollowed = Entry.UserInfo.IsFollowed;

        InitializeCommands();
        FollowCommand.RefreshFollowCommand(IsFollowed);
    }

    public string Username => Entry.UserInfo.Name;

    public long UserId => Entry.Id;

    public string AvatarUrl => Entry.UserInfo.ProfileImageUrls.Medium;
}
