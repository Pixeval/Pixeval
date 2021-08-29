using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Util.UI
{
    public interface INavigationModeInfo
    {
        NavigationMode? NavigationMode { get; }

        NavigationMode? GetNavigationModeAndReset();
    }
}