// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using FluentIcons.Common;

namespace Pixeval.AppManagement;

public record DownloadSettingsGroup
{
    [SettingsEntry(Symbol.Rename, AppSettingsResources.DownloadPathMacroEntry.Header, AppSettingsResources.DownloadPathMacroEntry.Description)]
    public string DownloadPathMacro { get; set; } = Path.Join(
        GetSpecialFolder(),
        "@{is_group?@{group_id}:}",
        "@{is_pic_set?[@{artist_name}] @{title}:}",
        "[@{artist_name}] @{id}@{is_pic_set?p@{pic_set_index}:}.@{ext}"
    );

    [SettingsEntry(Symbol.TextPeriodAsterisk, AppSettingsResources.WorkDownloadFormatEntry.Header, AppSettingsResources.WorkDownloadFormatEntry.Description)]
    public string IllustrationDownloadFormat { get; set; } = Models.Download.IllustrationDownloadFormatToken.DefaultToken;

    public string UgoiraDownloadFormat { get; set; } = Models.Download.UgoiraDownloadFormatToken.DefaultToken;

    public string NovelDownloadFormat { get; set; } = Models.Download.NovelDownloadFormatToken.DefaultToken;

    [SettingsEntry(Symbol.ImageSplit, AppSettingsResources.OverwriteDownloadedFileEntry.Header, AppSettingsResources.OverwriteDownloadedFileEntry.Description)]
    public bool OverwriteDownloadedFile { get; set; }

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(Symbol.DeveloperBoardLightning, AppSettingsResources.MaxDownloadConcurrencyLevelEntry.Header, AppSettingsResources.MaxDownloadConcurrencyLevelEntry.Description)]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 4;

    [JsonIgnore]
    [SettingsEntry(Symbol.FolderSync, AppSettingsResources.WorkSubscriptionsSettingsEntry.Header, AppSettingsResources.WorkSubscriptionsSettingsEntry.Description)]
    public byte WorkSubscriptions => 0;

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.None);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? Path.Join(picDirectory!,
                $"@{{is_novel?{Path.GetFileName(docPath)}:{Path.GetFileName(picPath)}}}")
            : $"@{{is_novel?{docPath}:{picPath}}}";
    }
}
