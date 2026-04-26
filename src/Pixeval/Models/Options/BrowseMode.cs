using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum BrowseMode
{
    [LocalizedResource(BrowseExperienceResources.Swipe)]
    Swipe,

    [LocalizedResource(BrowseExperienceResources.Continuous)]
    Continuous
}