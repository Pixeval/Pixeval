// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement.Settings;

public record AutoPlaySettings
{
    [SettingsEntry(Symbol.SlideMultipleArrowRight, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayIntervalEntry.Description)]
    public int IllustrationViewerAutoPlayInterval { get; set; } = 5;

    [SettingsEntry(Symbol.ArrowShuffle, AppSettingsResources.IllustrationViewerAutoPlayModeEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayModeEntry.Description)]
    public IllustrationViewerAutoPlayMode IllustrationViewerAutoPlayMode { get; set; }

    [SettingsEntry(Symbol.ImageMultiple, AppSettingsResources.IllustrationViewerAutoPlayScopeEntry.Header, AppSettingsResources.IllustrationViewerAutoPlayScopeEntry.Description)]
    public IllustrationViewerAutoPlayScope IllustrationViewerAutoPlayScope { get; set; }
}
