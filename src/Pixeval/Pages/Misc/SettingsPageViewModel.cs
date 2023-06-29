#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SettingsPageViewModel.cs
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

using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Setting.UI;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Misc;
using Pixeval.UserControls.TokenInput;
using WinUI3Utilities.Attributes;
using IllustrationViewModel = Pixeval.UserControls.IllustrationView.IllustrationViewModel;

namespace Pixeval.Pages.Misc;

[SettingsViewModel<AppSetting>(nameof(_appSetting))]
public partial class SettingsPageViewModel : ObservableObject
{
    public static readonly IEnumerable<string> AvailableFonts = new InstalledFontCollection().Families.Select(f => f.Name);

    public static readonly ICollection<Token> AvailableIllustMacros;

    private readonly AppSetting _appSetting;

    static SettingsPageViewModel()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();
        AvailableIllustMacros = factory.PathParser.MacroProvider.AvailableMacros
            .Select(m => $"@{{{(m is IMacro<IllustrationViewModel>.IPredicate ? $"{m.Name}:" : m.Name)}}}")
            .Select(s => new Token(s, false, false))
            .ToList();
    }

    public SettingsPageViewModel(AppSetting appSetting)
    {
        _appSetting = appSetting;
    }


    [DefaultValue(false)]
    public bool DisableDomainFronting
    {
        get => _appSetting.DisableDomainFronting;
        set => SetProperty(_appSetting.DisableDomainFronting, value, _appSetting, (setting, value) =>
        {
            setting.DisableDomainFronting = value;
            App.AppViewModel.MakoClient.Configuration.Bypass = !value;
        });
    }

    [DefaultValue(null)]
    public string? MirrorHost
    {
        get => _appSetting.MirrorHost;
        set => SetProperty(_appSetting.MirrorHost, value, _appSetting, (setting, value) =>
        {
            setting.MirrorHost = value;
            App.AppViewModel.MakoClient.Configuration.MirrorHost = value;
        });
    }

    [DefaultValue(typeof(DownloadConcurrencyDefaultValueProvider))]
    public int MaxDownloadTaskConcurrencyLevel
    {
        get => _appSetting.MaxDownloadTaskConcurrencyLevel;
        set => SetProperty(_appSetting.MaxDownloadTaskConcurrencyLevel, value, _appSetting, (setting, value) =>
        {
            App.AppViewModel.DownloadManager.ConcurrencyDegree = value;
            setting.MaxDownloadTaskConcurrencyLevel = value;
        });
    }

    public DateTimeOffset GetMinSearchEndDate(DateTimeOffset startDate)
    {
        return startDate.AddDays(1);
    }

    public string GetLastUpdateCheckDisplayString(DateTimeOffset lastChecked)
    {
        return $"{SettingsPageResources.LastCheckedPrefix}{lastChecked.ToString(CultureInfo.CurrentUICulture)}";
    }

    public void ResetDefault()
    {
        foreach (var propertyInfo in typeof(SettingsPageViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            propertyInfo.SetValue(this, propertyInfo.GetDefaultValue());
        }
    }

    public void ClearData<T, TModel>(ClearDataKind kind, IPersistentManager<T, TModel> manager) where T : new()
    {
        manager.Clear();
        ShowAndHide(kind switch
        {
            ClearDataKind.BrowseHistory => SettingsPageResources.BrowseHistoriesCleared,
            ClearDataKind.SearchHistory => SettingsPageResources.SearchHistoriesCleared,
            ClearDataKind.DownloadHistory => SettingsPageResources.DownloadHistoriesCleared,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        });
    }

    #region SnackBar

    [ObservableProperty] private string _snackBarTitle = "";

    [ObservableProperty] private string _snackBarSubtitle = "";

    [ObservableProperty] private FontIconSource _snackBarIconSource = null!;

    [ObservableProperty] private bool _isSnackBarOpen;

    /// <remarks>
    /// Value type members require property to enable thread sharing
    /// </remarks>
    private static DateTime HideSnakeBarTime { get; set; }

    /// <summary>
    /// Show SnackBar
    /// </summary>
    /// <param name="message"><see cref="TeachingTip.Title"/></param>
    /// <param name="severity"><see cref="TeachingTip.IconSource"/></param>
    /// <param name="hint"><see cref="TeachingTip.Subtitle"/></param>
    public void Show(string message, Severity severity = Severity.Ok, string hint = "")
    {
        SnackBarTitle = message;
        SnackBarSubtitle = hint;
        SnackBarIconSource = new()
        {
            Glyph = severity switch
            {
                Severity.Ok => "\xE10B", // Accept
                Severity.Information => "\xE946", // Info
                Severity.Important => "\xE171", // Important
                Severity.Warning => "\xE7BA", // Warning
                Severity.Error => "\xEA39", // ErrorBadge
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<Severity, string>(severity)
            }
        };

        IsSnackBarOpen = true;
    }

    /// <summary>
    /// Show SnackBar and hide after <paramref name="mSec"/> microseconds
    /// </summary>
    /// <param name="message"><see cref="TeachingTip.Title"/></param>
    /// <param name="severity"><see cref="TeachingTip.IconSource"/></param>
    /// <param name="hint"><see cref="TeachingTip.Subtitle"/></param>
    /// <param name="mSec">Automatically hide after <paramref name="mSec"/> milliseconds</param>
    public async void ShowAndHide(string message, Severity severity = Severity.Ok, string hint = "", int mSec = 1500)
    {
        HideSnakeBarTime = DateTime.Now + TimeSpan.FromMilliseconds(mSec - 100);

        Show(message, severity, hint);

        await Task.Delay(mSec);

        if (DateTime.Now > HideSnakeBarTime)
            IsSnackBarOpen = false;
    }

    /// <summary>
    /// Snack bar severity on <see cref="TeachingTip.IconSource"/> (Segoe Fluent Icons font)
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Accept (E10B)
        /// </summary>
        Ok,
        /// <summary>
        /// Info (E946)
        /// </summary>
        Information,
        /// <summary>
        /// Important (E171)
        /// </summary>
        Important,
        /// <summary>
        /// Warning (E7BA)
        /// </summary>
        Warning,
        /// <summary>
        /// ErrorBadge (EA39)
        /// </summary>
        Error
    }

    #endregion
}
