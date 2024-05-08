#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public sealed partial class IllustratorItemViewModel : EntryViewModel<User>
{
    [ObservableProperty]
    private bool _isFollowed;

    public IllustratorItemViewModel(User user) : base(user)
    {
        IsFollowed = Entry.UserInfo.IsFollowed;

        InitializeCommands();
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    public string Username => Entry.UserInfo.Name;

    public long UserId => Entry.Id;

    public string AvatarUrl => Entry.UserInfo.ProfileImageUrls.Medium;
}
