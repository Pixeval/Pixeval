// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Model;
using Pixeval.Util;

namespace Pixeval.Controls;

public abstract partial class WorkEntryViewModel<T> : ThumbnailEntryViewModel<T>, IWorkViewModel where T : class, IWorkEntry
{
    protected WorkEntryViewModel(T entry) : base(entry)
    {
        IsBookmarkedDisplay = Entry.IsFavorite ? HeartButtonState.Checked : HeartButtonState.Unchecked;
        InitializeCommands();
    }

    IWorkEntry IWorkViewModel.Entry => Entry;

    public int TotalFavorite => Entry.TotalFavorite;

    public int TotalView => Entry.TotalView;

    public bool IsFavorite
    {
        get => Entry.IsFavorite;
        set => Entry.IsFavorite = value;
    }

    [ObservableProperty]
    public partial HeartButtonState IsBookmarkedDisplay { get; set; }

    partial void OnIsBookmarkedDisplayChanged(HeartButtonState value)
    {
        if (value is HeartButtonState.Pending)
            return;
        IsFavorite = value is HeartButtonState.Checked;
    }

    public IReadOnlyList<Tag> Tags => Entry.Tags;

    public string Title => Entry.Title;

    public string Description => Entry.Description;

    public UserEntity User => Entry.User;

    public DateTimeOffset CreateDate => Entry.CreateDate;

    public ImageUrls ThumbnailUrls => Entry.ThumbnailUrls;

    public AiType AiType => Entry.AiType;

    public XRestrict XRestrict => Entry.XRestrict;

    public bool IsAiGenerated => Entry.AiType is AiType.AiGenerated;

    public bool IsXRestricted => Entry.XRestrict is not XRestrict.Ordinary;

    public bool IsPrivate => Entry.IsPrivate;

    public bool IsMuted => Entry.IsMuted;

    public BadgeMode XRestrictionCaption =>
        Entry.XRestrict switch
        {
            XRestrict.R18G => BadgeMode.R18G,
            _ => BadgeMode.R18
        };

    protected override string ThumbnailUrl => Entry.GetThumbnailUrl();
}
