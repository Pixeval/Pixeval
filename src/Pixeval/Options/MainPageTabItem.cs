#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MainPageTabItem.cs
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

using Microsoft.UI.Xaml.Controls;
using Pixeval.Misc;
using Pixeval.Pages;

namespace Pixeval.Options
{
    /// <summary>
    ///     We require a strict matching between the value of the enum member and the order of the
    ///     <see cref="NavigationViewItem" />
    ///     in <see cref="MainPage" />
    /// </summary>
    public enum MainPageTabItem
    {
        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabDailyRecommendations))]
        DailyRecommendation = 0,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabRanking))]
        Ranking = 1,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabBookmark))]
        Bookmark = 2,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabFollow))]
        Follow = 3,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabSpotlight))]
        Spotlight = 4,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabFeed))]
        Feed = 5,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabUpdate))]
        Update = 6,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabReverseSearch))]
        ReverseSearch = 7
    }
}