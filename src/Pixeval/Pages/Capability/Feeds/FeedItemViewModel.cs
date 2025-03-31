// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Util.UI;
using Windows.UI.ViewManagement;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability.Feeds;

static file class FeedItemColors
{
    public static readonly SolidColorBrush AddBookmark = new(C.ToAlphaColor(0xFF5449));
    public static readonly SolidColorBrush AddFavorite = new(C.ToAlphaColor(0x85976E));
    public static readonly SolidColorBrush PostIllust = new(C.ToAlphaColor(0x769CDF));
    public static readonly SolidColorBrush AddNovelBookmark = new(C.ToAlphaColor(0x9B9168));
}

public interface IFeedEntry : IIdEntry
{
    public record SparseFeedEntry(Feed Entry) : IFeedEntry
    {
        public long Id => Entry.Id;
    }

    public record CondensedFeedEntry(List<Feed?> Entries) : IFeedEntry
    {
        public long Id => Entries[0]?.Id ?? 0;
    }
}

public abstract class AbstractFeedItemViewModel(IFeedEntry entry) : EntryViewModel<IFeedEntry>(entry), IFactory<IFeedEntry, AbstractFeedItemViewModel>
{
    public SolidColorBrush FeedBrush => GetMostSignificantEntry()!.Type switch
    {
        FeedType.AddBookmark => FeedItemColors.AddBookmark,
        FeedType.AddFavorite => FeedItemColors.AddFavorite,
        FeedType.PostIllust => FeedItemColors.PostIllust,
        FeedType.AddNovelBookmark => FeedItemColors.AddNovelBookmark,
        _ => ThrowHelper.ArgumentOutOfRange<FeedType, SolidColorBrush?>(default)
    };

    public abstract ImageSource? UserAvatar { get; protected set; }

    public abstract SolidColorBrush ItemBackground { get; set; }

    public abstract string PostUsername { get; }

    public abstract string PostDateFormatted { get; }

    public bool IsCondensed => Entry is IFeedEntry.CondensedFeedEntry;

    public abstract Task LoadAsync();

    public void Select(bool value)
    {
        var selectedBackground = App.AppViewModel.AppSettings.ActualTheme is ElementTheme.Dark
            ? new UISettings().GetColorValue(UIColorType.AccentDark3)
            : new UISettings().GetColorValue(UIColorType.AccentLight3);
        ItemBackground = value ? new SolidColorBrush(selectedBackground) : new SolidColorBrush(Colors.Transparent);
    }

    public static AbstractFeedItemViewModel CreateInstance(IFeedEntry entry)
    {
        return entry switch
        {
            IFeedEntry.SparseFeedEntry(var feed) => new FeedItemSparseViewModel(feed),
            IFeedEntry.CondensedFeedEntry condensed => new FeedItemCondensedViewModel(condensed.Entries),
            _ => ThrowHelper.ArgumentOutOfRange<IFeedEntry, AbstractFeedItemViewModel?>(entry)
        };
    }

    /// <summary>
    /// Reify the entry from IFeedEntry.
    /// </summary>
    /// <returns></returns>
    public Feed? GetMostSignificantEntry()
    {
        return Entry switch
        {
            IFeedEntry.SparseFeedEntry(var feed) => feed,
            IFeedEntry.CondensedFeedEntry condensed => condensed.Entries.First(),
            _ => ThrowHelper.ArgumentOutOfRange<IFeedEntry, Feed?>(Entry)
        };
    }
}
