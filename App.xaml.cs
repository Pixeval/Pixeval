// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Persisting;
using Pixeval.UI;
using Refit;

#if RELEASE
using Pixeval.Objects.Exceptions.Logger;
#endif

namespace Pixeval
{
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            var serviceCollection = new ServiceCollection();
            Pixeval.Startup.ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            if (Dispatcher != null)
                Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if RELEASE
            ExceptionLogger.WriteException(e);
#elif DEBUG
            if (e is ApiException apiException) MessageBox.Show(apiException.Content);
#endif
        }


        private async void  App_OnStartup(object sender, StartupEventArgs e)
        {
            await Settings.Global.Restore();
            var mainWindow = _serviceProvider.GetService<MainWindow>();
        }

        private async void App_OnExit(object sender, ExitEventArgs e)
        {
            await Settings.Global.Store();
            if (!AppContext.LogoutExit && Identity.Global.AccessToken != null) await Identity.Global.Store();
        }
    }
}