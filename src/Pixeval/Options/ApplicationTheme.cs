using Pixeval.Misc;

namespace Pixeval.Options
{
    public enum ApplicationTheme
    {
        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ApplicationThemeDark))]
        Dark,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ApplicationThemeLight))]
        Light,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ApplicationThemeSystemDefault))]
        SystemDefault
    }
}