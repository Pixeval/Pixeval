// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.AppManagement;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Models.Options;
using Pixeval.ViewModels;

namespace Pixeval.Views;

public class PixevalSettings : ViewModelBase
{
    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public static WorkType WorkType => Settings.SearchSettings.WorkType;

    public static SimpleWorkType SimpleWorkType => Settings.SearchSettings.DefaultSimpleWorkType;

    public static ThumbnailLayoutType LayoutType => Settings.BrowsingExperienceSettings.ThumbnailLayoutType;

    public static TokenUser Me => App.AppViewModel.MakoClient.Me!;

    public static long MyId => Me.Id;

    public static PixevalSettings Instance { get; } = new();

    public bool IsLoggedIn => App.AppViewModel.MakoClient.Me is not null;

    public void OnIsLoggedInChanged() => OnPropertyChanged(nameof(IsLoggedIn));

    public bool OpenWorkInfo
    {
        get => Settings.BrowsingExperienceSettings.OpenWorkInfoByDefault;
        set => SetProperty(Settings.BrowsingExperienceSettings.OpenWorkInfoByDefault, value, Settings.BrowsingExperienceSettings, (setting, v) =>
        {
            setting.OpenWorkInfoByDefault = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }

    public bool OpenUserInfo
    {
        get => Settings.BrowsingExperienceSettings.OpenUserInfoByDefault;
        set => SetProperty(Settings.BrowsingExperienceSettings.OpenUserInfoByDefault, value, Settings.BrowsingExperienceSettings, (setting, v) =>
        {
            setting.OpenUserInfoByDefault = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }

    public bool HideHomePageCardTitle
    {
        get => Settings.ApplicationSettings.HideHomePageCardTitle;
        set => SetProperty(Settings.ApplicationSettings.HideHomePageCardTitle, value, Settings.ApplicationSettings, (setting, v) =>
        {
            setting.HideHomePageCardTitle = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }

    public bool HideHomePageToolbar
    {
        get => Settings.ApplicationSettings.HideHomePageToolbar;
        set => SetProperty(Settings.ApplicationSettings.HideHomePageToolbar, value, Settings.ApplicationSettings, (setting, v) =>
        {
            setting.HideHomePageToolbar = v;
            AppInfo.SaveAppSettings(Settings);
        });
    }
}
