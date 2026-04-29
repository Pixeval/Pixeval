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
