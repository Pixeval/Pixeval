// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Mako.Model;
using Pixeval.Util.UI;
using Windows.System;

namespace Pixeval.Controls;

public partial class WorkEntryViewModel<T>
{
    /// <summary>
    /// Parameter: <see cref="ValueTuple{T1, T2, T3}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="IEnumerable{T}"/> where T is <see cref="string"/></description></item>
    /// <item><term>T2</term><description><see cref="bool"/>?</description></item>
    /// <item><term>T3</term><description><see cref="object"/> see <see cref="SaveCommand"/>'s parameter</description></item>
    /// </list>
    /// </summary>
    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);

    /// <summary>
    /// Parameter: <see cref="object"/> see <see cref="SaveCommand"/>'s parameter
    /// </summary>
    public XamlUICommand BookmarkCommand { get; } = "".GetCommand(Symbol.Heart, VirtualKeyModifiers.Control, VirtualKey.D);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/></description></item>
    /// <item><term>T2</term><description><see cref="GetImageStreams"/>?(<see cref="IllustrationItemViewModel"/>)</description></item>
    /// <item><term>T2</term><description><see cref="NovelContent"/>?(<see cref="NovelItemViewModel"/>)</description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="FrameworkElement"/><br/>
    /// Parameter3: <see langword="null"/>
    /// </summary>
    public XamlUICommand SaveCommand { get; } = EntryItemResources.Save.GetCommand(Symbol.Save, VirtualKeyModifiers.Control, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/></description></item>
    /// <item><term>T2</term><description><see cref="GetImageStreams"/>?(<see cref="IllustrationItemViewModel"/>)</description></item>
    /// <item><term>T2</term><description><see cref="NovelContent"/>?(<see cref="NovelItemViewModel"/>)</description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="FrameworkElement"/>
    /// </summary>
    public XamlUICommand SaveAsCommand { get; } = EntryItemResources.SaveAs.GetCommand(Symbol.SaveEdit, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    /// <summary>
    /// <see cref="IllustrationItemViewModel"/>:<br/>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/></description></item>
    /// <item><term>T2</term><description><see cref="GetImageStreams"/></description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="GetImageStreams"/><br/>
    /// <see cref="NovelItemViewModel"/>:<br/>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/>
    /// <list type="bullet">
    /// <item><term>T1</term><description><see cref="FrameworkElement"/></description></item>
    /// <item><term>T2</term><description><see cref="NovelContent"/></description></item>
    /// </list>
    /// 
    /// Parameter2: <see cref="NovelContent"/>
    /// </summary>
    public XamlUICommand CopyCommand { get; } = EntryItemResources.Copy.GetCommand(Symbol.Copy);

    public XamlUICommand OpenUserInfoPage { get; } = EntryItemResources.OpenUserInfoPage.GetCommand(Symbol.Person);

    private void InitializeCommands()
    {
        InitializeCommandsBase();

        AddToBookmarkCommand.ExecuteRequested += AddToBookmarkCommandOnExecuteRequested;

        BookmarkCommand.RefreshBookmarkCommand(IsFavorite, false);
        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;

        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;
    }

    private async void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (IsBookmarkedDisplay is HeartButtonState.Pending)
            return;
        IsBookmarkedDisplay = HeartButtonState.Pending; // pre-update
        BookmarkCommand.RefreshBookmarkCommand(IsFavorite, true);
        var result = await SetBookmarkAsync(Id, !IsFavorite);
        IsBookmarkedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
        BookmarkCommand.RefreshBookmarkCommand(result, false);
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked && result)
            SaveCommand.Execute(args.Parameter);
    }

    private async void AddToBookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not (IEnumerable<string> userTags, bool isPrivate, var parameter) || IsBookmarkedDisplay is HeartButtonState.Pending)
            return;
        IsBookmarkedDisplay = HeartButtonState.Pending; // pre-update
        BookmarkCommand.RefreshBookmarkCommand(IsFavorite, true);
        var result = await SetBookmarkAsync(Id, !IsFavorite, isPrivate, userTags);
        IsBookmarkedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
        BookmarkCommand.RefreshBookmarkCommand(result, false);
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked)
            SaveCommand.Execute(parameter);
    }

    protected abstract Task<bool> SetBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null);

    protected abstract void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);
}
