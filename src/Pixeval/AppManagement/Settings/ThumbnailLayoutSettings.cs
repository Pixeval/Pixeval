// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement.Settings;

public record ThumbnailLayoutSettings
{
    public ThumbnailLayoutType ThumbnailLayoutType { get; set; } = ThumbnailLayoutType.LinedFlow;

    [SettingsEntry(Symbol.AutoFitHeight, AppSettingsResources.IllustrationLinedFlowItemHeightEntry.Header, AppSettingsResources.IllustrationLinedFlowItemHeightEntry.Description)]
    public int IllustrationLinedFlowItemHeight { get; set; } = 200;

    [SettingsEntry(Symbol.AutoFitWidth, AppSettingsResources.IllustrationGridItemSizeEntry.Header, AppSettingsResources.IllustrationGridItemSizeEntry.Description)]
    public int IllustrationGridItemSize { get; set; } = 150;

    [SettingsEntry(Symbol.AutoFitHeight, AppSettingsResources.IllustrationGridLineSizeEntry.Header, AppSettingsResources.IllustrationGridLineSizeEntry.Description)]
    public int IllustrationGridLineSize { get; set; } = 200;

    [SettingsEntry(Symbol.AutoFitWidth, AppSettingsResources.IllustrationMasonryColumnWidthEntry.Header, AppSettingsResources.IllustrationMasonryColumnWidthEntry.Description)]
    public int IllustrationMasonryColumnWidth { get; set; } = 250;
}
