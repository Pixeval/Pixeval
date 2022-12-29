using System;
using Pixeval.Navigation;
using Pixeval.Pages;

namespace Pixeval.Services.Navigation
{
    internal class MainPageNavigationService : NavigationService<MainPage>
    {
        public MainPageNavigationService(IServiceProvider serviceProvider, MainPage mainPage, RoutesBuilder<MainPage> routesBuilder) : base(serviceProvider, mainPage, routesBuilder)
        {
        }
    }
}
