// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections;
using System.Collections.Generic;
using Pixeval.Controls;

namespace Pixeval.Util;

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

        var result = x.PublishDate.CompareTo(y.PublishDate);
        // 比较Id以保证稳定排序
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

        var result = x.TotalBookmarks.CompareTo(y.TotalBookmarks);
        // 比较Id以保证稳定排序
        return result is 0 ? x.Id.CompareTo(y.Id) : result;
    }
}
