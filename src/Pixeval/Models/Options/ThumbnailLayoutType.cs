// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ThumbnailLayoutType
{
    [LocalizedResource(BrowseExperienceResources.LinedFlow)]
    LinedFlow,

    [LocalizedResource(BrowseExperienceResources.Grid)]
    Grid,

    VerticalUniformStack,

    HorizontalUniformStack,

    VerticalStack,

    HorizontalStack,

    Staggered
}
