// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ThumbnailLayoutType
{
    [LocalizedResource(EnumResources.ThumbnailLayoutTypeLinedFlow)]
    LinedFlow,

    [LocalizedResource(EnumResources.ThumbnailLayoutTypeGrid)]
    Grid,

    [LocalizedResource(EnumResources.ThumbnailLayoutTypeMasonry)]
    Masonry,

    VerticalUniformStack,

    HorizontalUniformStack,

    VerticalStack,

    HorizontalStack
}
