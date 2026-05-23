// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public partial class UserItemViewModel : EntryViewModel<User>, IFactory<User, UserItemViewModel>
{
    public static UserItemViewModel CreateInstance(User entry) => new(entry);

    public UserItemViewModel(User user) : base(user)
    {
        IsFollowedDisplay = IsFollowed ? HeartButtonState.Checked : HeartButtonState.Unchecked;

        var workEntries = Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ToArray();
        if (workEntries.ElementAtOrDefault(0) is { } t0)
        {
            Banner0Url = t0.GetThumbnailUrl();
            if (workEntries.ElementAtOrDefault(1) is { } t1)
            {
                Banner1Url = t1.GetThumbnailUrl();
                if (workEntries.ElementAtOrDefault(2) is { } t2)
                    Banner2Url = t2.GetThumbnailUrl();
            }
        }
    }

    public bool IsFollowed => Entry.UserInfo.IsFollowed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFollowed))]
    public partial HeartButtonState IsFollowedDisplay { get; set; }

    public string Username => Entry.UserInfo.Name;

    public long UserId => Entry.Id;

    public string AvatarUrl => Entry.UserInfo.ProfileImageUrls.Medium;

    public string? Banner0Url { get; }

    public string? Banner1Url { get; }

    public string? Banner2Url { get; }
}
