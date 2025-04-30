// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Mako.Model;
using Misaki;
using Pixeval.Util.UI;
using Windows.System;

namespace Pixeval.Controls;

public abstract partial class WorkEntryViewModel<T> : ThumbnailEntryViewModel<T>, IWorkViewModel where T : class, IArtworkInfo
{
    protected WorkEntryViewModel(T entry) : base(entry)
    {
        IsBookmarkedDisplay = Entry.IsFavorite ? HeartButtonState.Checked : HeartButtonState.Unchecked;

        InitializeCommandsBase();

        // TODO 用更通用的收藏接口
        if (Entry is IWorkEntry)
        {
            AddToBookmarkCommand = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);
            AddToBookmarkCommand.ExecuteRequested += AddToBookmarkCommandOnExecuteRequested;

            BookmarkCommand = "".GetCommand(Symbol.Heart, VirtualKeyModifiers.Control, VirtualKey.D);
            BookmarkCommand.RefreshBookmarkCommand(IsFavorite, false);
            BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;
        }

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;

        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;
    }

    IArtworkInfo IWorkViewModel.Entry => Entry;

    public bool IsFavorite => Entry.IsFavorite;

    [ObservableProperty]
    public partial HeartButtonState IsBookmarkedDisplay { get; set; }

    public DateTimeOffset CreateDate => Entry.CreateDate;

    public BadgeMode SafeBadgeMode =>
        Entry.SafeRating switch
        {
            { IsR18G: true } => BadgeMode.R18G,
            { IsR18: true } => BadgeMode.R18,
            _ => BadgeMode.None
        };

    protected override string ThumbnailUrl => Entry.Thumbnails.PickClosestHeightFrame(300).ImageUri.OriginalString;
}
