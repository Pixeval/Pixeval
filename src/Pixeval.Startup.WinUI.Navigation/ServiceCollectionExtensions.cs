using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Startup.WinUI.Navigation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureRoutes<TNavigationRoot, TNavigationService>(this IServiceCollection services, Action<RoutesBuilder<TNavigationRoot>> builderAction) where TNavigationRoot : class, INavigationRoot where TNavigationService : class, INavigationService<TNavigationRoot>
        {
            var builder = new RoutesBuilder<TNavigationRoot>(services);
            builderAction.Invoke(builder);
            foreach (var route in builder.Routes)
            {
                services.AddSingleton(route.Key);
            }

            foreach (var viewModel in builder.ViewModels)
            {
                services.AddSingleton(viewModel);
            }
            services.AddSingleton<RoutesBuilder<TNavigationRoot>>();
            services.AddSingleton<INavigationService<TNavigationRoot>, TNavigationService>();
            return services;
        }
    }
}
