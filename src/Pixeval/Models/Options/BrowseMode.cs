// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum BrowseMode
{
    [LocalizedResource(EnumResources.BrowseModeSwipe)]
    Swipe,

    [LocalizedResource(EnumResources.BrowseModeContinuous)]
    Continuous
}
