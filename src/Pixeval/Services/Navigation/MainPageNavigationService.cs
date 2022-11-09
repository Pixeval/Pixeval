using System;
using Pixeval.Pages;
using Pixeval.Startup.WinUI.Navigation;

namespace Pixeval.Services.Navigation
{
    internal class MainPageNavigationService : NavigationService<MainPage>
    {
        public MainPageNavigationService(IServiceProvider serviceProvider, MainPage mainPage, RoutesBuilder<MainPage> routesBuilder) : base(serviceProvider, mainPage, routesBuilder)
        {
        }
    }
}
