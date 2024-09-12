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
using Windows.UI.ViewManagement;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Timeline;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Pixeval.Pages.Capability.Feeds;

public partial class FeedItemViewModel(Feed entry) : EntryViewModel<Feed>(entry), IViewModelFactory<Feed, FeedItemViewModel>
{
    [ObservableProperty]
    private SolidColorBrush? _itemBackground = new(Colors.Transparent);

    public void Select(bool value)
    {
        ItemBackground = value ? new SolidColorBrush(new UISettings().GetColorValue(UIColorType.AccentLight3)) : new SolidColorBrush(Colors.Transparent);
    }

    [ObservableProperty] 
    private TimelineAxisPlacement _placement;

    // If the post date is within one day, show the precise moment, otherwise shows the date
    // we make an optimistic assumption that user rarely view feeds over one year ago, so
    // we don't show the year here.
    public string PostDateFormatted =>
        (DateTime.Now - Entry.PostDate) < TimeSpan.FromDays(1)
            ? Entry.PostDate.ToString("hh:mm tt")
            : Entry.PostDate.ToString("M");

    public Symbol Icon => entry.Type switch
    {
        FeedType.AddBookmark => Symbol.Heart,
        FeedType.AddFavorite => Symbol.People,
        FeedType.AddIllust => Symbol.Fire,
        FeedType.AddNovelBookmark => Symbol.BookAdd,
        _ => throw new ArgumentOutOfRangeException()
    };

    public SolidColorBrush IconSecondaryBrush => entry.Type switch
    {
        FeedType.AddBookmark => new SolidColorBrush(UiHelper.ParseHexColor("#FF5449")),
        FeedType.AddFavorite => new SolidColorBrush(UiHelper.ParseHexColor("#85976E")),
        FeedType.AddIllust => new SolidColorBrush(UiHelper.ParseHexColor("#8991A2")),
        FeedType.AddNovelBookmark => new SolidColorBrush(UiHelper.ParseHexColor("#9B9168")),
        _ => throw new ArgumentOutOfRangeException()
    };

    public SolidColorBrush IconBackground => entry.Type switch
    {
        FeedType.AddBookmark => new SolidColorBrush(UiHelper.ParseHexColor("#B33B15")),
        FeedType.AddFavorite => new SolidColorBrush(UiHelper.ParseHexColor("#63A002")),
        FeedType.AddIllust => new SolidColorBrush(UiHelper.ParseHexColor("#769CDF")),
        FeedType.AddNovelBookmark => new SolidColorBrush(UiHelper.ParseHexColor("#FFDE3F")),
        _ => throw new ArgumentOutOfRangeException()
    };

    [ObservableProperty]
    private ImageSource _userAvatar = null!;

    public static FeedItemViewModel CreateInstance(Feed entry, int index)
    {
        return new FeedItemViewModel(entry);
    }

    public virtual async Task LoadAsync()
    {
        Placement = TimelineAxisPlacement.Left; // index % 2 == 0 ? TimelineAxisPlacement.Left : TimelineAxisPlacement.Right;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable MVVMTK0034
        if (_userAvatar is not null)
#pragma warning restore MVVMTK0034
            return;

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

