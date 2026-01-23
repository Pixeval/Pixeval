// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public partial class UserItemViewModel : EntryViewModel<Mako.Model.User>, IFactory<Mako.Model.User, UserItemViewModel>, IDisposable
{
    public static UserItemViewModel CreateInstance(Mako.Model.User entry) => new(entry);

    public UserItemViewModel(Mako.Model.User user) : base(user)
    {
        IsFollowedDisplay = IsFollowed ? HeartButtonState.Checked : HeartButtonState.Unchecked;
    }

    public bool IsFollowed => Entry.UserInfo.IsFollowed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFollowed))]
    public partial HeartButtonState IsFollowedDisplay { get; set; }

    public string Username => Entry.UserInfo.Name;

    public long UserId => Entry.Id;

    public string AvatarUrl => Entry.UserInfo.ProfileImageUrls.Medium;
}
