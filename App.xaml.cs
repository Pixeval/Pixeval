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
using Pixeval.Persisting;
using Refit;

#if RELEASE
using Pixeval.Objects.Exceptions.Logger;
#endif

namespace Pixeval
{
    public partial class App
    {
        public App()
        {
            if (Dispatcher != null)
                Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if RELEASE
            if (!(e is NullReferenceException))
            {
                if (e is ApiException apiException)
                {
                    ExceptionLogger.WriteException(apiException);
                    return;
                }

                ExceptionLogger.WriteException(e);
            }
#elif DEBUG
            if (e is ApiException apiException) MessageBox.Show(apiException.Content);
#endif
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Settings.Global.Restore();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Settings.Global.Store();
            if (!PixevalEnvironment.LogoutExit && Identity.Global.AccessToken != null) await Identity.Global.Store();
            base.OnExit(e);
        }
    }
}