// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Mako.Model;
using Pixeval.Controls;

namespace Pixeval.Util;

public class WorkEntryPublishDateComparer : IComparer<IWorkEntry>
{
    public static readonly WorkEntryPublishDateComparer Instance = new();

    public int Compare(IWorkEntry? x, IWorkEntry? y)
    {
        if (x is null || y is null)
            return 0;

        var result = x.CreateDate.CompareTo(y.CreateDate);
        // 比较Id以保证稳定排序
        return result is 0 ? x.Id.CompareTo(y.Id) : result;
    }
}

public class WorkEntryBookmarkComparer : IComparer<IWorkEntry>
{
    public static readonly WorkEntryBookmarkComparer Instance = new();

    public int Compare(IWorkEntry? x, IWorkEntry? y)
    {
        if (x is null || y is null)
            return 0;

        var result = x.TotalFavorite.CompareTo(y.TotalFavorite);
        // 比较Id以保证稳定排序
        return result is 0 ? x.Id.CompareTo(y.Id) : result;
    }
}
