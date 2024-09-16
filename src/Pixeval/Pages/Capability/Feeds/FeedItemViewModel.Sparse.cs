#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FeedItemSparseViewModel.cs
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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Timeline;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Util;
using System.Threading.Tasks;
using System;
using Microsoft.UI;
using Pixeval.AppManagement;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability.Feeds;

public partial class FeedItemSparseViewModel(Feed entry) : AbstractFeedItemViewModel(new IFeedEntry.SparseFeedEntry(entry)), IFactory<Feed, FeedItemSparseViewModel>
{
    [ObservableProperty]
    private TimelineAxisPlacement _placement;

    public override string PostUsername => entry.PostUsername ?? string.Empty;

    // If the post date is within one day, show the precise moment, otherwise shows the date
    // we make an optimistic assumption that user rarely view feeds over one year ago, so
    // we don't show the year here.
    public override string PostDateFormatted =>
        (DateTime.Now - entry.PostDate) < TimeSpan.FromDays(1)
            ? entry.PostDate.ToString("hh:mm tt")
            : entry.PostDate.ToString("M");

    private ImageSource? _userAvatar;

    // It's impossible to use [ObservableProperty] here, for that generated properties lack the `override` modifier
    // same for the ItemBackground property
    public override ImageSource UserAvatar
    {
        get => _userAvatar!;
        protected set => SetProperty(ref _userAvatar, value);
    }

    private SolidColorBrush _itemBackground = new(Colors.Transparent);

    public override SolidColorBrush ItemBackground
    {
        get => _itemBackground;
        set => SetProperty(ref _itemBackground, value);
    }

    public static FeedItemSparseViewModel CreateInstance(Feed entry)
    {
        return new FeedItemSparseViewModel(entry);
    }

    public override async Task LoadAsync()
    {
        Placement = TimelineAxisPlacement.Left; // index % 2 == 0 ? TimelineAxisPlacement.Left : TimelineAxisPlacement.Right;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable MVVMTK0034
        if (_userAvatar is not null)
#pragma warning restore MVVMTK0034
            return;

        if (entry.PostUserThumbnail is { } url)
        {
            var image = (await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(url, 35)).UnwrapOrElse(await AppInfo.ImageNotAvailable)!;
            UserAvatar = image;
        }
        else
        {
            UserAvatar = await AppInfo.ImageNotAvailable;
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override Uri AppUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.PostIllust => MakoHelper.GenerateIllustrationAppUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserAppUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelAppUri(entry.Id),
        _ => ThrowHelper.ArgumentOutOfRange<FeedType?, Uri>(entry.Type)
    };

    public override Uri WebUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.PostIllust => MakoHelper.GenerateIllustrationWebUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserWebUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelWebUri(entry.Id),
        _ => ThrowHelper.ArgumentOutOfRange<FeedType?, Uri>(entry.Type)
    };

    public override Uri PixEzUri => entry.Type switch
    {
        FeedType.AddBookmark or FeedType.PostIllust => MakoHelper.GenerateIllustrationPixEzUri(entry.Id),
        FeedType.AddFavorite => MakoHelper.GenerateUserPixEzUri(entry.Id),
        FeedType.AddNovelBookmark => MakoHelper.GenerateNovelPixEzUri(entry.Id),
        _ => ThrowHelper.ArgumentOutOfRange<FeedType?, Uri>(entry.Type)
    };
}
