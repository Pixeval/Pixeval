using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Misc
{
    public interface INavigationModeInfo
    {
        NavigationMode? NavigationMode { get; }

        NavigationMode? GetNavigationModeAndReset();
    }
}