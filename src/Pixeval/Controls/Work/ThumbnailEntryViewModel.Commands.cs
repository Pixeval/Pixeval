#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ThumbnailEntryViewModel.Commands.cs
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
using System.IO;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Util.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Controls;

public partial class ThumbnailEntryViewModel<T>
{
    /// <summary>
    /// Parameter: <see cref="ValueTuple{T1, T2, T3}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="IEnumerable{T}"/> where T is <see cref="string"/></description></item>
    /// <item><term>T2</term><description><see cref="bool"/>?</description></item>
    /// <item><term>T3</term><description><see cref="object"/> see <see cref="SaveCommand"/>'s parameter</description></item>
    /// </list>
    /// </summary>
    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(FontIconSymbol.BookmarksE8A4);

    /// <summary>
    /// Parameter: <see cref="object"/> see <see cref="SaveCommand"/>'s parameter
    /// </summary>
    public XamlUICommand BookmarkCommand { get; } = "".GetCommand(FontIconSymbol.HeartEB51, VirtualKeyModifiers.Control, VirtualKey.D);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/>?</description></item>
    /// <item><term>T2</term><description><see cref="Func{T, TResult}"/>?
    /// <list type="bullet">
    /// <item><term>T</term><description><see cref="IProgress{T}"/>?</description></item>
    /// <item><term>TResult</term><description><see cref="Stream"/>?</description></item>
    /// </list>
    /// </description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="FrameworkElement"/>?
    /// </summary>
    public XamlUICommand SaveCommand { get; } = EntryItemResources.Save.GetCommand(FontIconSymbol.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="Window"/></description></item>
    /// <item><term>T2</term><description><see cref="Func{T, TResult}"/>?
    /// <list type="bullet">
    /// <item><term>T</term><description><see cref="IProgress{T}"/>?</description></item>
    /// <item><term>TResult</term><description><see cref="Stream"/>?</description></item>
    /// </list>
    /// </description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="Window"/>
    /// </summary>
    public XamlUICommand SaveAsCommand { get; } = EntryItemResources.SaveAs.GetCommand(FontIconSymbol.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/>?</description></item>
    /// <item><term>T2</term><description><see cref="Func{T, TResult}"/>?
    /// <list type="bullet">
    /// <item><term>T</term><description><see cref="IProgress{T}"/>?</description></item>
    /// <item><term>TResult</term><description><see cref="Stream"/>? </description></item>
    /// </list>
    /// </description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="ValueTuple{T1, T2}"/>
    /// <see cref="Func{T, TResult}"/>?
    /// <list type="bullet">
    /// <item><term>T</term><description><see cref="IProgress{T}"/>?</description></item>
    /// <item><term>TResult</term><description><see cref="Stream"/>?</description></item>
    /// </list>
    /// </summary>
    public XamlUICommand CopyCommand { get; } = EntryItemResources.Copy.GetCommand(FontIconSymbol.CopyE8C8, VirtualKeyModifiers.Control, VirtualKey.C);

    private void InitializeCommands()
    {
        InitializeCommandsBase();

        AddToBookmarkCommand.ExecuteRequested += AddToBookmarkCommandOnExecuteRequested;

        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;

        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;
    }

    private async void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsBookmarked = await SetBookmarkAsync(Id, !IsBookmarked);
        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked && IsBookmarked)
            SaveCommand.Execute(args.Parameter);
    }

    private async void AddToBookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not (IEnumerable<string> userTags, bool isPrivate, var parameter))
            return;
        var success = await SetBookmarkAsync(Id, true, isPrivate, userTags);
        if (!success)
            return;
        IsBookmarked = true;
        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked)
            SaveCommand.Execute(parameter);
    }

    protected abstract Task<bool> SetBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null);

    protected abstract void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);
}
