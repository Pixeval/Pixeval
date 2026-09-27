// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia.Layout;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement.Settings;

public record BrowsingExperienceSettingsGroup
{
    [SettingsEntry(Symbol.GlanceHorizontal, AppSettingsResources.ThumbnailLayoutTypeEntry.Header, AppSettingsResources.ThumbnailLayoutTypeEntry.Description)]
    public ThumbnailLayoutSettings ThumbnailLayout { get; set; } = new();

    [SettingsEntry(Symbol.CardUiPortraitFlip, AppSettingsResources.BrowseMode.Header, AppSettingsResources.BrowseMode.Description)]
    public BrowseMode BrowseMode { get; set; } = BrowseMode.Swipe;

    [SettingsEntry(Symbol.ArrowBetweenDown, AppSettingsResources.BrowseDirection.Header, AppSettingsResources.BrowseDirection.Description)]
    public Orientation BrowseDirection { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, AppSettingsResources.TargetAPIPlatformEntry.Header, AppSettingsResources.TargetAPIPlatformEntry.Description)]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.TagDismiss, AppSettingsResources.BlockedTagsEntry.Header, AppSettingsResources.BlockedTagsEntry.Description, AppSettingsResources.BlockedTagsEntry.Placeholder)]
    public ObservableCollection<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Pin, AppSettingsResources.PinnedTagsEntry.Header, AppSettingsResources.PinnedTagsEntry.Description, AppSettingsResources.PinnedTagsEntry.Placeholder)]
    public ObservableCollection<string> PinnedTags { get; set; } = [];

    [JsonIgnore]
    [SettingsEntry(Symbol.PersonProhibited, AppSettingsResources.BlockedUsersEntry.Header, AppSettingsResources.BlockedUsersEntry.Description)]
    public byte BlockedUsers => 0;

    [SettingsEntry(Symbol.Info, AppSettingsResources.OpenWorkInfoByDefaultEntry.Header, AppSettingsResources.OpenWorkInfoByDefaultEntry.Description)]
    public bool OpenWorkInfoByDefault { get; set; }

    [SettingsEntry(Symbol.PersonInfo, AppSettingsResources.OpenUserInfoByDefaultEntry.Header, AppSettingsResources.OpenUserInfoByDefaultEntry.Description)]
    public bool OpenUserInfoByDefault { get; set; } = true;

    [SettingsEntry(Symbol.SlidePlay, AppSettingsResources.AutoPlayEntry.Header, AppSettingsResources.AutoPlayEntry.Description)]
    public AutoPlaySettings AutoPlay { get; set; } = new();
}
