using System;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    public XamlUICommand FollowCommand { get; } = "".GetCommand(Symbol.Person);

    private void InitializeCommands()
    {
        InitializeCommandsBase();

        FollowCommand.RefreshFollowCommand(IsFollowed);
        FollowCommand.ExecuteRequested += FollowCommandExecuteRequested;
    }

    private async void FollowCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = await MakoHelper.SetFollowAsync(UserId, !IsFollowed);
        FollowCommand.RefreshFollowCommand(IsFollowed);
    }

    public override Uri AppUri => MakoHelper.GenerateUserAppUri(UserId);

    public override Uri WebUri => MakoHelper.GenerateUserWebUri(UserId);

    public override Uri PixEzUri => MakoHelper.GenerateUserPixEzUri(UserId);
}
