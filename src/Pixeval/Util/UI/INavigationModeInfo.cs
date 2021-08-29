using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Util.UI
{
    public interface INavigationModeInfo
    {
        static abstract NavigationMode? NavigationMode { get; }

        static abstract NavigationMode? GetNavigationModeAndReset();
    }
}