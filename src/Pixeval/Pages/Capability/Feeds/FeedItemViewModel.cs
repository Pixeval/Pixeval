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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Timeline;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Pixeval.Pages.Capability.Feeds;

public class BookmarkIllustFeedItemViewModel(Feed entry) : FeedItemViewModel(entry)
{

}

public partial class FeedItemViewModel(Feed entry) : EntryViewModel<Feed>(entry), IViewModelFactory<Feed, FeedItemViewModel>
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
        return new FeedItemViewModel(entry) { Placement = index % 2 == 0 ? TimelineAxisPlacement.Right : TimelineAxisPlacement.Left };
    }

    public async Task LoadAsync()
    {
        if (entry.PostUserThumbnail is { } url)
        {
            var image = (await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(url, 35)).UnwrapOrThrow();
            UserAvatar = image;
        }
        else
        {
            UserAvatar = AppInfo.ImageNotAvailable.Value;
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

