// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Controls;

namespace Pixeval.Util;

public class WorkEntryPublishDateComparer : IComparer<IWorkViewModel>
{
    public static readonly WorkEntryPublishDateComparer Instance = new();

    public int Compare(IWorkViewModel? x, IWorkViewModel? y)
    {
        if (x is null || y is null)
            return 0;

        var result = x.Entry.CreateDate.CompareTo(y.Entry.CreateDate);
        // 比较Hash以保证稳定排序
        return result is 0 ? x.Entry.GetHashCode().CompareTo(y.Entry.GetHashCode()) : result;
    }
}

public class WorkEntryBookmarkComparer : IComparer<IWorkViewModel>
{
    public static readonly WorkEntryBookmarkComparer Instance = new();

    public int Compare(IWorkViewModel? x, IWorkViewModel? y)
    {
        if (x is null || y is null)
            return 0;

        var result = x.Entry.TotalFavorite.CompareTo(y.Entry.TotalFavorite);
        // 比较Hash以保证稳定排序
        return result is 0 ? x.Entry.GetHashCode().CompareTo(y.Entry.GetHashCode()) : result;
    }
}
