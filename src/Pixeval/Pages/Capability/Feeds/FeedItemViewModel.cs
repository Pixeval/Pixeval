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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Util.UI;
using WinUI3Utilities;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Pixeval.Pages.Capability.Feeds;

static file class FeedItemColors
{
    public static readonly SolidColorBrush AddBookmark = new(UiHelper.ParseHexColor("#FF5449"));
    public static readonly SolidColorBrush AddFavorite = new(UiHelper.ParseHexColor("#85976E"));
    public static readonly SolidColorBrush PostIllust = new(UiHelper.ParseHexColor("#769CDF"));
    public static readonly SolidColorBrush AddNovelBookmark = new(UiHelper.ParseHexColor("#9B9168"));
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

    public abstract ImageSource UserAvatar { get; protected set; }

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
