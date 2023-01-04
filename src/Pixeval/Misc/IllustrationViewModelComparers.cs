﻿#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationViewModelComparers.cs
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

using System.Collections;
using System.Collections.Generic;
using Pixeval.UserControls;
using IllustrationViewModel = Pixeval.UserControls.IllustrationView.IllustrationViewModel;

namespace Pixeval.Misc;

public class IllustrationViewModelPublishDateComparer : IComparer<IllustrationViewModel>, IComparer
{
    public static readonly IllustrationViewModelPublishDateComparer Instance = new();

    public int Compare(object? x, object? y)
    {
        return Compare(x as IllustrationViewModel, y as IllustrationViewModel);
    }

    public int Compare(IllustrationViewModel? x, IllustrationViewModel? y)
    {
        if (x is null || y is null)
        {
            return 0;
        }

        return x.PublishDate.CompareTo(y.PublishDate);
    }
}

public class IllustrationBookmarkComparer : IComparer<IllustrationViewModel>, IComparer
{
    public static readonly IllustrationBookmarkComparer Instance = new();

    public int Compare(object? x, object? y)
    {
        return Compare(x as IllustrationViewModel, y as IllustrationViewModel);
    }

    public int Compare(IllustrationViewModel? x, IllustrationViewModel? y)
    {
        if (x?.Illustration is { } xi && y?.Illustration is { } yi)
        {
            return xi.TotalBookmarks.CompareTo(yi.TotalBookmarks);
        }

        return 0;
    }
}