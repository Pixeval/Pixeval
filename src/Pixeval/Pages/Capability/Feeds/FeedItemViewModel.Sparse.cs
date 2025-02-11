// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Timeline;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using System.Threading.Tasks;
using System;
using Microsoft.UI;
using WinUI3Utilities;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Pages.Capability.Feeds;

public partial class FeedItemSparseViewModel(Feed entry) : AbstractFeedItemViewModel(new IFeedEntry.SparseFeedEntry(entry)), IFactory<Feed, FeedItemSparseViewModel>
{
    [ObservableProperty]
    public partial TimelineAxisPlacement Placement { get; set; }

    public override string PostUsername => entry.PostUsername ?? string.Empty;

    // If the post date is within one day, show the precise moment, otherwise show only the date
    // we make an optimistic assumption that the users will rarely view those feeds over one year ago, so
    // we don't show the year here.
    public override string PostDateFormatted =>
        (DateTime.Now - entry.PostDate) < TimeSpan.FromDays(1)
            ? entry.PostDate.ToString("hh:mm tt")
            : entry.PostDate.ToString("M");

    [ObservableProperty]
    public override partial ImageSource? UserAvatar { get; protected set; }

    [ObservableProperty]
    public override partial SolidColorBrush ItemBackground { get; set; } = new(Colors.Transparent);

    public static FeedItemSparseViewModel CreateInstance(Feed entry)
    {
        return new FeedItemSparseViewModel(entry);
    }

    public override async Task LoadAsync()
    {
        Placement = TimelineAxisPlacement.Left; // index % 2 == 0 ? TimelineAxisPlacement.Left : TimelineAxisPlacement.Right;

        if (UserAvatar is not null)
            return;

        if (entry.PostUserThumbnail is { } url)
            UserAvatar = await CacheHelper.GetSourceFromCacheAsync(url, desiredWidth: 35);
        else
            UserAvatar = await CacheHelper.ImageNotAvailableTask.Value;
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
