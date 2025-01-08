// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum ThumbnailDirection
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ThumbnailDirectionLandscape))]
    Landscape,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ThumbnailDirectionPortrait))]
    Portrait
}
