﻿#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainPageTabItem.cs
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
using Pixeval.Attributes;

using Pixeval.Pages;

namespace Pixeval.Options;

/// <summary>
///     We require a strict matching between the value of the enum member and the order of the
///     <see cref="NavigationViewItem" />in <see cref="MainPage" />
/// </summary>
public enum MainPageTabItem
{
    DailyRecommendation = 0,
    Ranking = 1,
    Bookmark = 2,
    Follow = 3,
    Spotlight = 4,
    Feed = 5,
    Update = 6,
    ReverseSearch = 7
}