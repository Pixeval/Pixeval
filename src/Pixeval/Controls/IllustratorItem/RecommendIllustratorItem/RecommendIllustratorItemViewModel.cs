#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendIllustratorProfileViewModel.cs
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

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class RecommendIllustratorItemViewModel : UserItemViewModel<RecommendUser>
{

    [ObservableProperty]
    private Style _buttonStyle = Application.Current.Resources["AccentButtonStyle"].To<Style>();

    // this value does not have to be initialized to `true` or `false` because the recommend illustrators are guaranteed to be not followed yet.
    [ObservableProperty]
    private bool _isFollowed;

    public RecommendIllustratorItemViewModel(RecommendUser user, IEnumerable<long> ids) : base(user, new IllustratorIllustrationsOverviewViewModel(ids))
    {
        InitializeCommands();
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    public bool Premium => Entry.Premium;

    public string Username => Entry.Name;

    public override long UserId => Entry.Id;

    public override string AvatarUrl => Entry.Image;

    protected override void FollowCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = MakoHelper.SetFollow(UserId, !IsFollowed);
        ButtonStyle = Application.Current.Resources[IsFollowed ? "DefaultButtonStyle" : "AccentButtonStyle"].To<Style>();
        FollowCommand.GetFollowCommand(IsFollowed);
    }
}
