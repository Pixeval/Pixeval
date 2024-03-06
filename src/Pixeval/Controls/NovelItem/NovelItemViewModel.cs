#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelItemViewModel.cs
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
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public partial class NovelItemViewModel(Novel novel) : ThumbnailEntryViewModel<Novel>(novel), IBookmarkableViewModel
{
    public override string Title => Entry.Title;

    public string Caption => Entry.Caption;

    public override UserInfo User => Entry.User;

    public override long Id => Entry.Id;

    public int TextLength => Entry.TextLength;

    public override bool IsBookmarked
    {
        get => Entry.IsBookmarked;
        set => Entry.IsBookmarked = value;
    }

    public override int Bookmark => Entry.TotalBookmarks;

    public int TotalView => Entry.TotalView;

    public override Tag[] Tags => Entry.Tags;

    public override DateTimeOffset PublishDate => Entry.CreateDate;

    protected override string ThumbnailUrl => Entry.Cover.Medium;
}

public partial class NovelItemViewModel
{
    protected override void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void ShowQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void ShowPixEzQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void SaveCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void SaveAsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }

    protected override void CopyCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {

    }
}
