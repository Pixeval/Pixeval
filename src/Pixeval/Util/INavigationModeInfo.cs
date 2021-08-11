using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Util
{
    public interface INavigationModeInfo
    {
        NavigationMode? NavigationMode { get; }

        NavigationMode? GetNavigationModeAndReset();
    }
}