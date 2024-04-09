using System;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities.Controls;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    public XamlUICommand FollowCommand { get; } = "".GetCommand(IconGlyph.ContactE77B);

    private void InitializeCommands()
    {
        InitializeCommandsBase();

        FollowCommand.GetFollowCommand(IsFollowed);
        FollowCommand.ExecuteRequested += FollowCommandExecuteRequested;
    }

    private async void FollowCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = await MakoHelper.SetFollowAsync(UserId, !IsFollowed);
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    protected override Uri AppUri => MakoHelper.GenerateUserAppUri(UserId);

    protected override Uri WebUri => MakoHelper.GenerateUserWebUri(UserId);

    protected override Uri PixEzUri => MakoHelper.GenerateUserPixEzUri(UserId);
}
