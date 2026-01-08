// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum ThumbnailDirection
{
    [LocalizedResource(nameof(MiscResources.ThumbnailDirectionLandscape))]
    Landscape,

    [LocalizedResource(nameof(MiscResources.ThumbnailDirectionPortrait))]
    Portrait
}
