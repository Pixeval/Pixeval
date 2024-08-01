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
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Timeline;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Pixeval.Pages.Capability.Feeds;

public partial class BookmarkIllustFeedItemViewModel(Feed entry, int index) : FeedItemViewModel(entry, index)
{
    [ObservableProperty]
    private ImageSource _thumbnail = null!;

    public override async Task LoadAsync()
    {
        _ = base.LoadAsync();
        if (entry.FeedThumbnail is { } url)
        {
            Thumbnail = (await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(url, 800))
                .UnwrapOrElse(await AppInfo.ImageNotAvailable.ValueAsync)!;
        }
    }
}

public partial class BookmarkNovelFeedItemViewModel(Feed entry, int index) : FeedItemViewModel(entry, index)
{

}

public partial class PostIllustFeedItemViewMode(Feed entry, int index) : FeedItemViewModel(entry, index)
{

}

public partial class FollowUserFeedItemViewModel(Feed entry, int index) : FeedItemViewModel(entry, index)
{

}


public partial class FeedItemViewModel(Feed entry, int index) : EntryViewModel<Feed>(entry), IViewModelFactory<Feed, FeedItemViewModel>
{
    [ObservableProperty] 
    private TimelineAxisPlacement _placement;

    public string PostDateFormatted => Entry.PostDate.ToString("f");

    public Symbol Icon => entry.Type switch
    {
        FeedType.AddBookmark => Symbol.Heart,
        FeedType.AddFavorite => Symbol.People,
        FeedType.AddIllust => Symbol.Fire,
        FeedType.AddNovelBookmark => Symbol.BookAdd,
        _ => throw new ArgumentOutOfRangeException()
    };

    [ObservableProperty]
    private ImageSource _userAvatar = null!;

    public static FeedItemViewModel CreateInstance(Feed entry, int index)
    {
        FeedItemViewModel vm = entry.Type switch
        {
            FeedType.AddBookmark => new BookmarkIllustFeedItemViewModel(entry, index),
            FeedType.AddIllust => new PostIllustFeedItemViewMode(entry, index),
            FeedType.AddFavorite => new FollowUserFeedItemViewModel(entry, index),
            FeedType.AddNovelBookmark => new BookmarkNovelFeedItemViewModel(entry, index),
            _ => throw new ArgumentOutOfRangeException()
        };
        return vm;
    }

    public virtual async Task LoadAsync()
    {
        Placement = TimelineAxisPlacement.Left; // index % 2 == 0 ? TimelineAxisPlacement.Left : TimelineAxisPlacement.Right;
        if (entry.PostUserThumbnail is { } url)
        {
            var image = (await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(url, 35)).UnwrapOrElse(await AppInfo.ImageNotAvailable.ValueAsync)!;
            UserAvatar = image;
        }
        else
        {
            UserAvatar = await AppInfo.ImageNotAvailable.ValueAsync;
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override Uri AppUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.AddIllust => MakoHelper.GenerateIllustrationAppUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserAppUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelAppUri(entry.Id),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public override Uri WebUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.AddIllust => MakoHelper.GenerateIllustrationWebUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserWebUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelWebUri(entry.Id),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override Uri PixEzUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.AddIllust => MakoHelper.GenerateIllustrationPixEzUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserPixEzUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelPixEzUri(entry.Id),
        _ => throw new ArgumentOutOfRangeException()
    };
}

