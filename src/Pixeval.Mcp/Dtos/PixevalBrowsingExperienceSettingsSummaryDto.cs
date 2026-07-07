// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalBrowsingExperienceSettingsSummaryDto(
    string ThumbnailLayoutType,
    string BrowseMode,
    string BrowseDirection,
    int IllustrationViewerAutoPlayInterval,
    string IllustrationViewerAutoPlayMode,
    string IllustrationViewerAutoPlayScope,
    string TargetFilter,
    int BlockedTagCount,
    bool OpenWorkInfoByDefault,
    bool OpenUserInfoByDefault);