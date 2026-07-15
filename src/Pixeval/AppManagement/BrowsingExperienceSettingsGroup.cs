// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using Avalonia.Layout;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Options;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record BrowsingExperienceSettingsGroup
{
    [SettingsEntry(Symbol.GlanceHorizontal, ThumbnailLayoutTypeEntryHeader, ThumbnailLayoutTypeEntryDescription)]
    public ThumbnailLayoutType ThumbnailLayoutType { get; set; } = ThumbnailLayoutType.LinedFlow;

    [SettingsEntry(Symbol.CardUiPortraitFlip, BrowseModeHeader, BrowseModeDescription)]
    public BrowseMode BrowseMode { get; set; } = BrowseMode.Swipe;

    [SettingsEntry(Symbol.ArrowBetweenDown, BrowseDirectionHeader, BrowseDirectionDescription)]
    public Orientation BrowseDirection { get; set; } = Orientation.Horizontal;

    [SettingsEntry(Symbol.SlideMultipleArrowRight, IllustrationViewerAutoPlayIntervalEntryHeader, IllustrationViewerAutoPlayIntervalEntryDescription)]
    public int IllustrationViewerAutoPlayInterval { get; set; } = 5;

    [SettingsEntry(Symbol.ArrowShuffle, IllustrationViewerAutoPlayModeEntryHeader, IllustrationViewerAutoPlayModeEntryDescription)]
    public IllustrationViewerAutoPlayMode IllustrationViewerAutoPlayMode { get; set; }

    [SettingsEntry(Symbol.ImageMultiple, IllustrationViewerAutoPlayScopeEntryHeader, IllustrationViewerAutoPlayScopeEntryDescription)]
    public IllustrationViewerAutoPlayScope IllustrationViewerAutoPlayScope { get; set; }

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, TargetAPIPlatformEntryHeader, TargetAPIPlatformEntryDescription)]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.TagDismiss, BlockedTagsEntryHeader, BlockedTagsEntryDescription, BlockedTagsEntryPlaceholder)]
    public ObservableCollection<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Info, OpenWorkInfoByDefaultEntryHeader, OpenWorkInfoByDefaultEntryDescription)]
    public bool OpenWorkInfoByDefault { get; set; }

    [SettingsEntry(Symbol.PersonInfo, OpenUserInfoByDefaultEntryHeader, OpenUserInfoByDefaultEntryDescription)]
    public bool OpenUserInfoByDefault { get; set; } = true;
}
