using Pixeval.Misc;

namespace Pixeval.Options
{
    public enum ThumbnailDirection
    {
        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ThumbnailDirectionLandscape))]
        Landscape,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ThumbnailDirectionPortrait))]
        Portrait
    }
}