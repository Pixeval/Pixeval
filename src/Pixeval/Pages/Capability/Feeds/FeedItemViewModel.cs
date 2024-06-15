#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FeedItemViewModel.cs
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
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Pixeval.Controls;
using Pixeval.Controls.Timeline;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Pixeval.Pages.Capability.Feeds;

public abstract partial class FeedItemViewModel(Feed entry) : EntryViewModel<Feed>(entry), IViewModelFactory<Feed, FeedItemViewModel>
{
    [ObservableProperty]
    private TimelineAxisPlacement _placement;
    
    public abstract Symbol Icon { get; }

    public static FeedItemViewModel CreateInstance(Feed entry, int index)
    {
        return entry.Type switch
        {
            FeedType.AddBookmark => new NewBookmarkFeedItemViewModel(entry),
            FeedType.AddIllust => new NewPostFeedItemViewModel(entry),
            FeedType.AddFavorite => new FollowedArtistFeedItemViewModel(entry),
            FeedType.AddNovelBookmark => new NewNovelBookmarkFeedItemViewModel(entry),
            _ => throw new ArgumentOutOfRangeException()
        };

    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class NewPostFeedItemViewModel(Feed entry) : FeedItemViewModel(entry)
{
    public override Symbol Icon => Symbol.Fire;

    public override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(entry.Id);

    public override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(entry.Id);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(entry.Id);
}

public class NewBookmarkFeedItemViewModel(Feed entry) : FeedItemViewModel(entry)
{
    public override Symbol Icon => Symbol.Heart;

    public override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(entry.Id);

    public override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(entry.Id);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(entry.Id);
}

public class NewNovelBookmarkFeedItemViewModel(Feed entry) : FeedItemViewModel(entry)
{
    public override Symbol Icon => Symbol.BookAdd;

    public override Uri AppUri => MakoHelper.GenerateNovelAppUri(entry.Id);

    public override Uri WebUri => MakoHelper.GenerateNovelWebUri(entry.Id);

    public override Uri PixEzUri => MakoHelper.GenerateNovelPixEzUri(entry.Id);
}

public class FollowedArtistFeedItemViewModel(Feed entry) : FeedItemViewModel(entry)
{
    public override Symbol Icon => Symbol.People;

    public override Uri AppUri => MakoHelper.GenerateUserAppUri(entry.Id);

    public override Uri WebUri => MakoHelper.GenerateUserWebUri(entry.Id);

    public override Uri PixEzUri => MakoHelper.GenerateUserPixEzUri(entry.Id);
}
