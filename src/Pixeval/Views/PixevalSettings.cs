// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.AppManagement;
using Mako.Global.Enum;
using Pixeval.Models.Options;

namespace Pixeval.Views;

public static class PixevalSettings
{
    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public static WorkType WorkType => Settings.SearchSettings.WorkType;

    public static SimpleWorkType SimpleWorkType => Settings.SearchSettings.DefaultSimpleWorkType;

    public static ThumbnailLayoutType LayoutType => Settings.BrowsingExperienceSettings.ThumbnailLayoutType;
}
