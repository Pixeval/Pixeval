// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record BrowsingExperienceSettingsGroup
{
    [SettingsEntry(Symbol.GlanceHorizontal, AppSettingsResources.ThumbnailLayoutTypeEntryHeader, AppSettingsResources.ThumbnailLayoutTypeEntryDescription)]
    public ThumbnailLayoutType ThumbnailLayoutType { get; set; } = ThumbnailLayoutType.LinedFlow;

    [SettingsEntry(Symbol.CardUiPortraitFlip, AppSettingsResources.BrowseModeHeader, AppSettingsResources.BrowseModeDescription)]
    public BrowseMode BrowseMode { get; set; } = BrowseMode.Swipe;

    [SettingsEntry(Symbol.ArrowBetweenDown, AppSettingsResources.BrowseDirectionHeader, AppSettingsResources.BrowseDirectionDescription)]
    public BrowseDirection BrowseDirection { get; set; } = BrowseDirection.LeftRight;

    [SettingsEntry(Symbol.SlideMultipleArrowRight, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntryHeader, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntryDescription)]
    public int IllustrationViewerAutoPlayInterval { get; set; } = 5;

    [SettingsEntry(Symbol.ArrowShuffle, AppSettingsResources.IllustrationViewerAutoPlayModeEntryHeader, AppSettingsResources.IllustrationViewerAutoPlayModeEntryDescription)]
    public IllustrationViewerAutoPlayMode IllustrationViewerAutoPlayMode { get; set; }

    [SettingsEntry(Symbol.ImageMultiple, AppSettingsResources.IllustrationViewerAutoPlayScopeEntryHeader, AppSettingsResources.IllustrationViewerAutoPlayScopeEntryDescription)]
    public IllustrationViewerAutoPlayScope IllustrationViewerAutoPlayScope { get; set; }

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, AppSettingsResources.TargetAPIPlatformEntryHeader, AppSettingsResources.TargetAPIPlatformEntryDescription)]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.TagDismiss, AppSettingsResources.BlockedTagsEntryHeader, AppSettingsResources.BlockedTagsEntryDescription, AppSettingsResources.BlockedTagsEntryPlaceholder)]
    public ObservableCollection<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Info, AppSettingsResources.OpenWorkInfoByDefaultEntryHeader, AppSettingsResources.OpenWorkInfoByDefaultEntryDescription)]
    public bool OpenWorkInfoByDefault { get; set; }
}
