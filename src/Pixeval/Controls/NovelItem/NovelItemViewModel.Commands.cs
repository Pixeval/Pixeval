#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelItemViewModel.Commands.cs
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

using System;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using Pixeval.Util;

namespace Pixeval.Controls;

public partial class NovelItemViewModel
{
    protected override async void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsBookmarked = await MakoHelper.SetNovelBookmarkAsync(Id, !IsBookmarked);
        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked && IsBookmarked && !User.IsFollowed)
            SaveCommandOnExecuteRequested(sender, args);
    }

    protected override void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {

    }

    protected override void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {

    }

    protected override void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {

    }

    protected override Uri AppUri => MakoHelper.GenerateNovelAppUri(Id);

    protected override Uri WebUri => MakoHelper.GenerateNovelWebUri(Id);

    protected override Uri PixEzUri => MakoHelper.GenerateNovelPixEzUri(Id);
}
