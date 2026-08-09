// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using Avalonia.Layout;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record BrowsingExperienceSettingsGroup
{
    [SettingsEntry(Symbol.GlanceHorizontal, AppSettingsResources.ThumbnailLayoutTypeEntry.Header, AppSettingsResources.ThumbnailLayoutTypeEntry.Description)]
    public ThumbnailLayoutType ThumbnailLayoutType { get; set; } = ThumbnailLayoutType.LinedFlow;

    [SettingsEntry(Symbol.AutoFitHeight, AppSettingsResources.IllustrationLinedFlowItemHeightEntry.Header, AppSettingsResources.IllustrationLinedFlowItemHeightEntry.Description)]
    public int IllustrationLinedFlowItemHeight { get; set; } = 200;

    [SettingsEntry(Symbol.AutoFitWidth, AppSettingsResources.IllustrationGridItemSizeEntry.Header, AppSettingsResources.IllustrationGridItemSizeEntry.Description)]
    public int IllustrationGridItemSize { get; set; } = 150;

    [SettingsEntry(Symbol.AutoFitHeight, AppSettingsResources.IllustrationGridLineSizeEntry.Header, AppSettingsResources.IllustrationGridLineSizeEntry.Description)]
    public int IllustrationGridLineSize { get; set; } = 200;

    [SettingsEntry(Symbol.AutoFitWidth, AppSettingsResources.IllustrationMasonryColumnWidthEntry.Header, AppSettingsResources.IllustrationMasonryColumnWidthEntry.Description)]
    public int IllustrationMasonryColumnWidth { get; set; } = 250;

    [SettingsEntry(Symbol.CardUiPortraitFlip, AppSettingsResources.BrowseMode.Header, AppSettingsResources.BrowseMode.Description)]
    public BrowseMode BrowseMode { get; set; } = BrowseMode.Swipe;

    [SettingsEntry(Symbol.ArrowBetweenDown, AppSettingsResources.BrowseDirection.Header, AppSettingsResources.BrowseDirection.Description)]
    public Orientation BrowseDirection { get; set; } = Orientation.Horizontal;

    [SettingsEntry(Symbol.SlideMultipleArrowRight, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntry.Description)]
    public int IllustrationViewerAutoPlayInterval { get; set; } = 5;

    [SettingsEntry(Symbol.ArrowShuffle, AppSettingsResources.IllustrationViewerAutoPlayModeEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayModeEntry.Description)]
    public IllustrationViewerAutoPlayMode IllustrationViewerAutoPlayMode { get; set; }

    [SettingsEntry(Symbol.ImageMultiple, AppSettingsResources.IllustrationViewerAutoPlayScopeEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayScopeEntry.Description)]
    public IllustrationViewerAutoPlayScope IllustrationViewerAutoPlayScope { get; set; }

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, AppSettingsResources.TargetAPIPlatformEntry.Header, AppSettingsResources.TargetAPIPlatformEntry.Description)]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.TagDismiss, AppSettingsResources.BlockedTagsEntry.Header, AppSettingsResources.BlockedTagsEntry.Description, AppSettingsResources.BlockedTagsEntry.Placeholder)]
    public ObservableCollection<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Info, AppSettingsResources.OpenWorkInfoByDefaultEntry.Header, AppSettingsResources.OpenWorkInfoByDefaultEntry.Description)]
    public bool OpenWorkInfoByDefault { get; set; }

    [SettingsEntry(Symbol.PersonInfo, AppSettingsResources.OpenUserInfoByDefaultEntry.Header, AppSettingsResources.OpenUserInfoByDefaultEntry.Description)]
    public bool OpenUserInfoByDefault { get; set; } = true;
}
