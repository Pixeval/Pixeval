// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Global.Enum;
using Mako.Model;

namespace Pixeval.ViewModels;

public sealed class SeriesItemViewModel(Series series, SimpleWorkType workType) : ThumbnailEntryViewModel<Series>(series)
{
    public SimpleWorkType WorkType { get; } = workType;

    public override string ThumbnailUrl => Entry.CoverUrl;

    public override Uri AppUri => field ??= new($"pixeval://series/{(WorkType is SimpleWorkType.Novel ? "novel" : "illust")}/{Id}");

    public override Uri WebsiteUri => field ??= new(WorkType is SimpleWorkType.Novel
        ? $"https://www.pixiv.net/novel/series/{Entry.Id}"
        : $"https://www.pixiv.net/user/{Entry.User.Id}/series/{Entry.Id}");
}
