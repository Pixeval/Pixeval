using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.VisualStudio.Threading;

namespace Pixeval.Navigation
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

        public INavigablePage? Current => _currentNode?.Value;
        public bool CanGoForward => _currentNode?.Next is not null;
        public bool CanGoBack => _currentNode?.Previous is not null;
        public async Task GoForwardAsync()
        {
            if (CanGoForward)
            {
                await NavigateToAsync(_currentNode!.Next!.Value);
            }
        }

        public async Task GoBackAsync()
        {
            if (CanGoBack)
            {
                await NavigateToAsync(_currentNode!.Previous!.Value);
            }
        }

        public Task NavigateToAsync(string route, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            return NavigateToAsync(_navigationRoutes[route], parameter, infoOverride);
        }

        public async Task NavigateToAsync(INavigablePage page, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            var previousPage = _currentNode?.Value;
            previousPage?.OnNavigatingToAsync(_navigationRoot, page, parameter);
            await Navigating?.InvokeAsync(this, new NavigationEventArgs(previousPage, page));
            page.OnNavigatingFromAsync(_navigationRoot, previousPage, parameter);
            _navigationRoot.NavigationFrame.Content = page;
            _currentNode = _currentNode is null ? _navigationHistory.AddLast(page) : _navigationHistory.AddAfter(_currentNode, page);
            page.OnNavigatedFromAsync(_navigationRoot, previousPage, parameter);
            await Navigated?.InvokeAsync(this, new NavigationEventArgs(previousPage, page));
            previousPage?.OnNavigatedToAsync(_navigationRoot, page, parameter);
        }

        public event AsyncEventHandler<NavigationEventArgs>? Navigated;
        public event AsyncEventHandler<NavigationEventArgs>? Navigating;
    }
}
