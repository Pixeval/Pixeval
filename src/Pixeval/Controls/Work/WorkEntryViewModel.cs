#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IllustrationItemViewModel.Thumbnail.cs
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
using Pixeval.CoreApi.Model;
using Pixeval.Util;

namespace Pixeval.Controls;

public abstract partial class WorkEntryViewModel<T> : ThumbnailEntryViewModel<T>, IWorkViewModel where T : class, IWorkEntry
{
    protected WorkEntryViewModel(T entry) : base(entry) => InitializeCommands();

    IWorkEntry IWorkViewModel.Entry => Entry;

    public int TotalBookmarks => Entry.TotalBookmarks;

    public int TotalView => Entry.TotalView;

    public bool IsBookmarked
    {
        get => Entry.IsBookmarked;
        set => Entry.IsBookmarked = value;
    }

    public Tag[] Tags => Entry.Tags;

    public string Title => Entry.Title;

    public string Caption => Entry.Caption;

    public UserInfo User => Entry.User;

    public DateTimeOffset PublishDate => Entry.CreateDate;

    public bool IsAiGenerated => Entry.AiType is 2;

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
