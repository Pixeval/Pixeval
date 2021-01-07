#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion


using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Core;
using Pixeval.Core.Timeline;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
#if RELEASE
using System.Net;
using Pixeval.Objects.Exceptions.Logger;

#endif

namespace Pixeval
{
    public partial class App
    {
        private static readonly Mutex UniqueMutex = new Mutex(true, "Pixeval Mutex");

        public App()
        {
            if (Dispatcher != null)
            {
                Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            }
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if RELEASE
            if (e is WebException || e is TaskCanceledException)
            {
                return;
            }
            ExceptionDumper.WriteException(e);
#elif DEBUG
            throw e;
#endif
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            InitializeFolders();
            CheckMultipleProcess();
            await RestoreSettings();
            await CheckUpdate();
            base.OnStartup(e);
        }

        private static void InitializeFolders()
        {
            Directory.CreateDirectory(PixevalContext.ProjectFolder);
            Directory.CreateDirectory(PixevalContext.SettingsFolder);
            Directory.CreateDirectory(PixevalContext.ExceptionReportFolder);
        }

        private static void CheckMultipleProcess()
        {
            if (!UniqueMutex.WaitOne(0, false))
            {
                MessageBox.Show(AkaI18N.MultiplePixevalInstanceDetected, AkaI18N.MultiplePixevalInstanceDetectedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }
        }

        private static async Task CheckUpdate()
        {
            if (await PixevalContext.UpdateAvailable() && MessageBox.Show(AkaI18N.PixevalUpdateAvailable, AkaI18N.PixevalUpdateAvailableTitle, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start(@"updater\Pixeval.Updater.exe");
                Environment.Exit(0);
            }
        }

        private static async Task RestoreSettings()
        {
            await Settings.Load();
            BrowsingHistoryAccessor.GlobalLifeTimeScope = new BrowsingHistoryAccessor(200, PixevalContext.BrowseHistoryDatabase);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Settings.Global.Save();
            if (Session.Current != null && !Session.Current.AccessToken.IsNullOrEmpty())
            {
                await Session.Current.Save();
            }
            if (File.Exists(PixevalContext.BrowseHistoryDatabase))
            {
                BrowsingHistoryAccessor.GlobalLifeTimeScope.SetWritable();
                BrowsingHistoryAccessor.GlobalLifeTimeScope.Rewrite();
                BrowsingHistoryAccessor.GlobalLifeTimeScope.Dispose();
            }
            else
            {
                BrowsingHistoryAccessor.GlobalLifeTimeScope.EmergencyRewrite();
            }

            base.OnExit(e);
        }
    }
}