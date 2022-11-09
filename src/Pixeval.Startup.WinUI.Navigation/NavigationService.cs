using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Pixeval.Startup.WinUI.Navigation
{
    public class NavigationService<TNavigationRoot> : INavigationService<TNavigationRoot> where TNavigationRoot : class, INavigationRoot
    {
        private LinkedListNode<INavigablePage>? _currentNode;

        private readonly LinkedList<INavigablePage> _navigationHistory = new();

        private readonly SortedDictionary<string, INavigablePage> _navigationRoutes = new();

        private readonly TNavigationRoot _navigationRoot;

        public NavigationService(IServiceProvider serviceProvider, TNavigationRoot navigationRoot, RoutesBuilder<TNavigationRoot> routesBuilder)
        {
            _navigationRoot = navigationRoot;
            foreach (var route in routesBuilder.Routes)
            {
                _navigationRoutes.Add(route.Key, (INavigablePage)serviceProvider.GetRequiredService(route.Value));
            }
        }

        public Frame? Frame { get; set; }

        public INavigablePage Current => _currentNode.Value;
        public bool CanGoForward { get; }
        public bool CanGoBack { get; }
        public void GoForward()
        {
            throw new NotImplementedException();
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            NavigateTo(_navigationRoutes[route], parameter, infoOverride);
        }

        public void NavigateTo(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            var previousPage = _currentNode?.Value;
            _navigationRoot.NavigationFrame.Content = page;
            page.OnNavigatedFrom(_navigationRoot, previousPage, parameter);
        }

        public Task NavigateToAsync(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            return NavigateToAsync(_navigationRoutes[route], parameter, infoOverride);
        }

        public Task NavigateToAsync(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            var previousPage = _currentNode?.Value;
            _navigationRoot.NavigationFrame.Content = page;
            page.OnNavigatedFrom(_navigationRoot, previousPage, parameter);
            return Task.CompletedTask;
        }
    }
}
