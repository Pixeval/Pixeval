using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Navigation;

public interface INavigationRoot
{
    Frame NavigationFrame { get; }
}