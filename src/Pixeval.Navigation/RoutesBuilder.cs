using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Navigation
{
    public class RoutesBuilder<TNavigationRoot> where TNavigationRoot : class, INavigationRoot
    {
        private readonly IServiceCollection _services;
        public RoutesBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public Dictionary<string, Type> Routes { get; } = new();

        public HashSet<Type> ViewModels { get; } = new();

        public RoutesBuilder<TNavigationRoot> AddPageWithRoute<T>(string route) where T : class, INavigablePage
        {
            Routes.Add(route, typeof(T));
            _services.AddSingleton<T>();
            return this;
        }

        public RoutesBuilder<TNavigationRoot> AddPageWithRoute<T, TViewModel>(string route) where T : class, INavigablePage where TViewModel : class
        {
            Routes.Add(route, typeof(T));
            ViewModels.Add(typeof(TViewModel));
            _services.AddSingleton<T>();
            _services.AddSingleton<TViewModel>();
            return this;
        }

    }
}
