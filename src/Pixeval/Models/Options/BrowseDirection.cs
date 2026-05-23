// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum BrowseDirection
{
    [LocalizedResource(EnumResources.BrowseDirectionLeftRight)]
    LeftRight,

    [LocalizedResource(EnumResources.BrowseDirectionRightLeft)]
    RightLeft,

    [LocalizedResource(EnumResources.BrowseDirectionTopDown)]
    TopDown,

    [LocalizedResource(EnumResources.BrowseDirectionBottomUp)]
    BottomUp
}
