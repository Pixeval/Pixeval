// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.AppManagement;
using Mako.Global.Enum;
using Pixeval.Models.Options;

namespace Pixeval.Views;

public static class PixevalSettings
{
    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public static WorkType WorkType => Settings.WorkType;

    public static SimpleWorkType SimpleWorkType => Settings.DefaultSimpleWorkType;

    public static ThumbnailLayoutType LayoutType => Settings.ThumbnailLayoutType;
}
