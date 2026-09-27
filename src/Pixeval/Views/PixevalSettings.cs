// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.AppManagement.Settings;
using Pixeval.Models.Options;
using Pixeval.ViewModels;

namespace Pixeval.Views;

public class PixevalSettings : ViewModelBase
{
    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public static WorkType WorkType => Settings.SearchSettings.WorkType;

    public static SimpleWorkType SimpleWorkType => Settings.SearchSettings.DefaultSimpleWorkType;

    public static ThumbnailLayoutType LayoutType => Settings.BrowsingExperienceSettings.ThumbnailLayout.ThumbnailLayoutType;

    public static double IllustrationLinedFlowItemHeight => Settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationLinedFlowItemHeight;

    public static double IllustrationGridItemSize => Settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridItemSize;

    public static double IllustrationGridLineSize => Settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridLineSize;

    public static double IllustrationMasonryColumnWidth => Settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationMasonryColumnWidth;

    public static TokenUser Me => App.AppViewModel.MakoClient.Me!;

    public static long MyId => Me.Id;

    public static PixevalSettings Instance { get; } = new();

    public bool IsLoggedIn => App.AppViewModel.MakoClient.Me is not null;

    public void OnIsLoggedInChanged() => OnPropertyChanged(nameof(IsLoggedIn));

    public bool OpenWorkInfo
    {
        get => Settings.BrowsingExperienceSettings.OpenWorkInfoByDefault;
        set
        {
            // 仅更新设置
            Settings.BrowsingExperienceSettings.OpenWorkInfoByDefault = value;
            AppInfo.SaveAppSettings(Settings);
        }
    }

    public bool OpenUserInfo
    {
        get => Settings.BrowsingExperienceSettings.OpenUserInfoByDefault;
        set
        {
            // 仅更新设置
            Settings.BrowsingExperienceSettings.OpenUserInfoByDefault = value;
            AppInfo.SaveAppSettings(Settings);
        }
    }

    public bool HideHomePageCardTitle
    {
        get => Settings.ApplicationSettings.HomePage.HideHomePageCardTitle;
        set => SetProperty(Settings.ApplicationSettings.HomePage.HideHomePageCardTitle, value, Settings.ApplicationSettings.HomePage, (setting, v) =>
        {
            setting.HideHomePageCardTitle = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }

    public bool HideHomePageToolbar
    {
        get => Settings.ApplicationSettings.HomePage.HideHomePageToolbar;
        set => SetProperty(Settings.ApplicationSettings.HomePage.HideHomePageToolbar, value, Settings.ApplicationSettings.HomePage, (setting, v) =>
        {
            setting.HideHomePageToolbar = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }
}
