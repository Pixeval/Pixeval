// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum BrowseDirection
{
    [LocalizedResource(Symbol.ArrowRight, EnumResources.BrowseDirectionLeftRight)]
    LeftRight,

    [LocalizedResource(Symbol.ArrowLeft, EnumResources.BrowseDirectionRightLeft)]
    RightLeft,

    [LocalizedResource(Symbol.ArrowDown, EnumResources.BrowseDirectionTopDown)]
    TopDown,

    [LocalizedResource(Symbol.ArrowUp, EnumResources.BrowseDirectionBottomUp)]
    BottomUp
}
