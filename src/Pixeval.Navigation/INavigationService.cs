using Microsoft.UI.Xaml.Media.Animation;

namespace Pixeval.Navigation
{
    public interface INavigationService<TRoot> where TRoot : class, INavigationRoot
    {
        INavigablePage Current { get; }
        bool CanGoForward { get; }
        bool CanGoBack { get; }
        void GoForward();
        void GoBack();
        void NavigateTo(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
        void NavigateTo(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
        Task NavigateToAsync(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
        Task NavigateToAsync(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null);
    }
}
