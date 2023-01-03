using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.VisualStudio.Threading;

namespace Pixeval.Navigation;

public interface INavigationService<TRoot> where TRoot : class, INavigationRoot
{

    INavigablePage? Current { get; }
    bool CanGoForward { get; }
    bool CanGoBack { get; }
    Task GoForwardAsync();
    Task GoBackAsync();
    Task NavigateToAsync(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
    Task NavigateToAsync(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
    public event AsyncEventHandler<NavigationEventArgs>? Navigated;
    public event AsyncEventHandler<NavigationEventArgs>? Navigating;
}