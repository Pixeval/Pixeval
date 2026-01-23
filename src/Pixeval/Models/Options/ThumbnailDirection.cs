// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ThumbnailDirection
{
    [LocalizedResource(MiscResources.ThumbnailDirectionLandscape)]
    Landscape,

    [LocalizedResource(MiscResources.ThumbnailDirectionPortrait)]
    Portrait
}
