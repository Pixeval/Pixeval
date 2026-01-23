// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Global.Enum;
using Pixeval.AppManagement;

namespace Pixeval.Views;

public static class PixevalSettings
{
    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public static int WorkSortOptionIndex => (int) Settings.WorkSortOption;
    
    public static int WorkTypeIndex => (int) Settings.WorkType;
    
    public static int SimpleWorkTypeIndex => (int) Settings.SimpleWorkType;
    
    public static TargetFilter TargetFilter => Settings.TargetFilter;
    
    public static ItemsViewLayoutType LayoutType => Settings.ItemsViewLayoutType;
}
