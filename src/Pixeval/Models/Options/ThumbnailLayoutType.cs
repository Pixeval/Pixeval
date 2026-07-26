// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ThumbnailLayoutType
{
    [LocalizedResource(EnumResources.ThumbnailLayoutType.LinedFlow)]
    LinedFlow,

    [LocalizedResource(EnumResources.ThumbnailLayoutType.Grid)]
    Grid,

    [LocalizedResource(EnumResources.ThumbnailLayoutType.Masonry)]
    Masonry,

    VerticalUniformStack,

    HorizontalUniformStack,

    VerticalStack,

    HorizontalStack
}
