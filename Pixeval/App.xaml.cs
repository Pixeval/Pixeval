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
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Pixeval.Core;
using Pixeval.Objects;
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
        
        private static bool CheckWebViewVersion()
        {
            var regKey = Registry.LocalMachine.OpenSubKey(
                Environment.Is64BitOperatingSystem
                    ? "SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
                    : "SOFTWARE\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
            );
            return regKey != null && !(regKey.GetValue("pv") as string).IsNullOrEmpty();
        }

        private static async Task RootCaCertInstallation()
        {
            using var cert = await CertificateManager.GetFakeCaRootCertificate();
            var fakeCertMgr = new CertificateManager(cert);
            if (!fakeCertMgr.Query(StoreName.Root, StoreLocation.CurrentUser))
            {
                if (MessageBox.Show(AkaI18N.CertificateInstallationIsRequired, AkaI18N.CertificateInstallationIsRequiredTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    fakeCertMgr.Install(StoreName.Root, StoreLocation.CurrentUser);
                }
                else
                {
                    Environment.Exit(-1);
                }
            }
        }

        
        protected override async void OnStartup(StartupEventArgs e)
        {
            InitializeFolders();
            if (!CheckWebViewVersion())
            {
                MessageBox.Show(AkaI18N.WebView2DownloadIsRequired);
                Clipboard.SetText("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
                Environment.Exit(0);
            }
            await RootCaCertInstallation();
            CheckMultipleProcess();
            await RestoreSettings();
            await CheckUpdate();
            base.OnStartup(e);
        }

        private static void InitializeFolders()
        {
            Directory.CreateDirectory(AppContext.ProjectFolder);
            Directory.CreateDirectory(AppContext.SettingsFolder);
            Directory.CreateDirectory(AppContext.ExceptionReportFolder);
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
            if (await AppContext.UpdateAvailable() && MessageBox.Show(AkaI18N.PixevalUpdateAvailable, AkaI18N.PixevalUpdateAvailableTitle, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start(@"updater\Pixeval.Updater.exe");
                Environment.Exit(0);
            }
        }

        private static async Task RestoreSettings()
        {
            await Settings.Restore();
            BrowsingHistoryAccessor.GlobalLifeTimeScope = new BrowsingHistoryAccessor(200, AppContext.BrowseHistoryDatabase);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Settings.Global.Store();
            if (Session.Current != null && !Session.Current.AccessToken.IsNullOrEmpty())
            {
                await Session.Current.Store();
            }
            if (File.Exists(AppContext.BrowseHistoryDatabase))
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