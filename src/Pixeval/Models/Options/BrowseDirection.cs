using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum BrowseDirection
{
    [LocalizedResource(BrowseExperienceResources.LeftRight)]
    LeftRight,

    [LocalizedResource(BrowseExperienceResources.RightLeft)]
    RightLeft,

    [LocalizedResource(BrowseExperienceResources.TopDown)]
    TopDown,

    [LocalizedResource(BrowseExperienceResources.BottomUp)]
    BottomUp
}