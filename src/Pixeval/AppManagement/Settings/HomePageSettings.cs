// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;

namespace Pixeval.AppManagement.Settings;

public record HomePageSettings
{
    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageRowsEntry.Header, AppSettingsResources.HomePageRowsEntry.Description)]
    public int HomePageRows { get; set; } = 7;

    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageColumnsEntry.Header, AppSettingsResources.HomePageColumnsEntry.Description)]
    public int HomePageColumns { get; set; } = 1;

    [SettingsEntry(Symbol.WindowHeaderHorizontal, AppSettingsResources.HideHomePageToolbarEntry.Header, AppSettingsResources.HideHomePageToolbarEntry.Description)]
    public bool HideHomePageToolbar { get; set; }

    [SettingsEntry(Symbol.AppTitle, AppSettingsResources.HideHomePageCardTitleEntry.Header, AppSettingsResources.HideHomePageCardTitleEntry.Description)]
    public bool HideHomePageCardTitle { get; set; }
}
