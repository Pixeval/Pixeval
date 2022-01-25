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
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Pixeval.Core;
using Pixeval.Core.Persistent;
using Pixeval.Objects;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
#if RELEASE
using System.Net;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.UI.UserControls;

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
            switch (e)
            {
                case WebException _:
                case TaskCanceledException _:
                    return;
                case AuthenticateFailedException _:
                    MessageDialog.Warning(UI.MainWindow.Instance.WarningDialog, AkaI18N.AppApiAuthenticateTimeout);
                    break;
            }
            ExceptionDumper.WriteException(e);
#elif DEBUG
            Debug.WriteLine(e.ToString());
#endif
        }
        
        private static bool CheckWebViewVersion()
        {
            var regKey1 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}")?.GetValue("pv") as string;
            var regKey2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}")?.GetValue("pv") as string;
            return !regKey1.IsNullOrEmpty() || !regKey2.IsNullOrEmpty();
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
            await RestoreSettingsAndSession();
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
        private static async Task RestoreSettingsAndSession()
        {
            await Settings.Restore();
            await Session.Restore();
            BrowsingHistoryAccessor.GlobalLifeTimeScope = new BrowsingHistoryAccessor(200, PixevalContext.BrowseHistoryDatabase);
            FavoriteSpotlightAccessor.GlobalLifeTimeScope = new FavoriteSpotlightAccessor(PixevalContext.BrowseHistoryDatabase);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Settings.Store();
            if (Session.Current != null)
            {
                await Session.Current.Store();
            }
            if (File.Exists(PixevalContext.BrowseHistoryDatabase))
            {
                // TODO Dependency injection can magnificently clarify these template codes, But I'm planning to ship it on WinUI version instead of this old one
                BrowsingHistoryAccessor.GlobalLifeTimeScope.Rewrite();
                BrowsingHistoryAccessor.GlobalLifeTimeScope.Dispose();
                FavoriteSpotlightAccessor.GlobalLifeTimeScope.Rewrite();
                FavoriteSpotlightAccessor.GlobalLifeTimeScope.Dispose();
            }
            else
            {
                BrowsingHistoryAccessor.GlobalLifeTimeScope.EmergencyRewrite();
                FavoriteSpotlightAccessor.GlobalLifeTimeScope.EmergencyRewrite();
            }

            base.OnExit(e);
        }
    }
}