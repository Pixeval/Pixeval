#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewModelComparers.cs
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
using Pixeval.Controls;

namespace Pixeval.Misc;

public class WorkViewModelPublishDateComparer : IComparer<IWorkViewModel>, IComparer
{
    public static readonly WorkViewModelPublishDateComparer Instance = new();

    public int Compare(object? x, object? y)
    {
        return Compare(x as IWorkViewModel, y as IWorkViewModel);
    }

    public int Compare(IWorkViewModel? x, IWorkViewModel? y)
    {
        if (x is null || y is null)
            return 0;

        // 比较Id以保证稳定排序
        var result = x.PublishDate.CompareTo(y.PublishDate);
        return result is 0 ? x.Id.CompareTo(y.Id) : result;
    }
}

public class WorkViewModelBookmarkComparer : IComparer<IWorkViewModel>, IComparer
{
    public static readonly WorkViewModelBookmarkComparer Instance = new();

    public int Compare(object? x, object? y)
    {
        return Compare(x as IWorkViewModel, y as IWorkViewModel);
    }

    public int Compare(IWorkViewModel? x, IWorkViewModel? y)
    {
        if (x is null || y is null)
            return 0;

        // 比较Id以保证稳定排序
        var result = x.TotalBookmarks.CompareTo(y.TotalBookmarks);
        return result is 0 ? x.Id.CompareTo(y.Id) : result;
    }
}
