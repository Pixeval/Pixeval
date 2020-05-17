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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Caching;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Pixeval.Persisting.WebApi;
#if RELEASE
using Pixeval.Objects.Exceptions.Logger;

#elif DEBUG
using System.Globalization;
#endif

namespace Pixeval
{
    public partial class App
    {
        public App()
        {
#if DEBUG
            CultureInfo.CurrentCulture = new CultureInfo("zh-CN");
#endif
            if (Dispatcher != null)
                Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if RELEASE
            ExceptionDumper.WriteException(e);
#elif DEBUG
            MessageBox.Show(e.ToString());
#endif
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // These initializations ensure Pixeval to run properly, they MUST be initialized before everything start
            WriteToCurrentUserPathVariable();
            InitializeFolders();
            CheckCppRedistributable();
            CheckMultipleProcess();
            await InstallFakeCaCertificate();

            // These initializations are for WEB API LOGIN
            await WritePac();
            CefSharpInitialize();

            // These initializations handle USER SESSION AND PIXEVAL UPDATE
            await RestoreSettings();
            await CheckUpdate();

            // This initializations are for PROCESS COMMUNICATION AND PLUGGABLE PROTOCOL
            CreatePluggableProtocolRegistry();
            PluggableProtocolListener.StartServer();


            if (e.Args.Any()) await PluggableProtocolParser.Parse(e.Args[0]);
            base.OnStartup(e);
        }

        private static void WriteToCurrentUserPathVariable()
        {
            var target = EnvironmentVariableTarget.User;
            var pathOld = Environment.GetEnvironmentVariable("PATH", target);
            if (pathOld == null)
            {
                target = EnvironmentVariableTarget.Machine;
                Environment.GetEnvironmentVariable("PATH", target);
            }

            var paths = pathOld!.Split(';');
            var location = Path.GetDirectoryName(typeof(App).Assembly.Location);
            if (paths.All(p => p != location))
                Environment.SetEnvironmentVariable("PATH",
                    pathOld.EndsWith(';') ? pathOld + location : $"{pathOld};{location}", target);
        }

        private static void InitializeFolders()
        {
            Directory.CreateDirectory(AppContext.ProjectFolder);
            Directory.CreateDirectory(AppContext.SettingsFolder);
            Directory.CreateDirectory(AppContext.ExceptionReportFolder);
            Directory.CreateDirectory(AppContext.ResourceFolder);
            Directory.CreateDirectory(AppContext.PermanentlyFolder);
        }

        private static void CreatePluggableProtocolRegistry()
        {
            var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\pixeval");
            if (regKey == null)
            {
                using var rootKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\pixeval");
                if (rootKey == null) throw new InvalidOperationException(nameof(rootKey));
                rootKey.SetValue("", "URL:Pixeval Protocol");
                rootKey.SetValue("URL Protocol", "");
                using var shellKey = rootKey.CreateSubKey("shell\\open\\command");
                if (shellKey == null) throw new InvalidOperationException(nameof(shellKey));
                shellKey.SetValue("",
                    $"{Path.Combine(AppContext.ProjectFolder, "Interchange", "Pixeval.Interchange.exe")} %1");
            }

            regKey?.Dispose();
        }

        private static void CheckMultipleProcess()
        {
            if (Process.GetProcessesByName(AppContext.AppIdentifier).Length > 1)
            {
                MessageBox.Show(AkaI18N.MultiplePixevalInstanceDetected, AkaI18N.MultiplePixevalInstanceDetectedTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }
        }

        private static void CheckCppRedistributable()
        {
            if (!CppRedistributableInstalled())
            {
                MessageBox.Show(AkaI18N.CppRedistributableRequired, AkaI18N.CppRedistributableRequiredTitle,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Clipboard.SetText(
                    "https://support.microsoft.com/zh-cn/help/2977003/the-latest-supported-visual-c-downloads");
                Environment.Exit(-1);
            }
        }

        private static async Task CheckUpdate()
        {
            if (await AppContext.UpdateAvailable())
                if (MessageBox.Show(AkaI18N.PixevalUpdateAvailable, AkaI18N.PixevalUpdateAvailableTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(@"updater\Pixeval.AutoUpdater.exe");
                    Environment.Exit(0);
                }
        }

        /// <summary>
        ///     Check if the required Visual C++ Redistributable is installed on the computer
        /// </summary>
        /// <returns>Cpp redistributable is installed</returns>
        private static bool CppRedistributableInstalled()
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            if (key == null) return false;

            var success = int.TryParse(key.GetValue("Bld").ToString(), out var version);
            // visual C++ redistributable Bld table: 
            // version   v2015    v2017    v2019
            // ----------------------------------
            //   Bid     23026    26020    27820
            const int vc2015Bld = 23026;
            return success && version >= vc2015Bld;
        }

        private static void CefSharpInitialize()
        {
            Cef.Initialize(new CefSettings
            {
                CefCommandLineArgs =
                {
                    {"proxy-pac-url", "http://127.0.0.1:4321/pixeval_pac.pac"}
                }
            }, true, browserProcessHandler: null);
        }

        private static async Task InstallFakeCaCertificate()
        {
            var certificateManager = new CertificateManager(await CertificateManager.GetFakeCaRootCertificate());
            if (!certificateManager.Query(StoreName.Root, StoreLocation.CurrentUser))
            {
                if (MessageBox.Show(AkaI18N.CertificateInstallationIsRequired,
                    AkaI18N.CertificateInstallationIsRequiredTitle, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    certificateManager.Install(StoreName.Root, StoreLocation.CurrentUser);
                else Environment.Exit(-1);
            }
        }

        private static async Task RestoreSettings()
        {
            await Settings.Restore();
            AppContext.DefaultCacheProvider = Settings.Global.CachingPolicy == CachingPolicy.Memory
                ? (IWeakCacheProvider<BitmapImage, Illustration>) MemoryCache<BitmapImage, Illustration>.Shared
                : new FileCache<BitmapImage, Illustration>(AppContext.CacheFolder, image => image.ToStream(),
                    InternalIO.CreateBitmapImageFromStream);
            AppContext.DefaultCacheProvider.Clear();
            BrowsingHistoryAccessor.GlobalLifeTimeScope =
                new BrowsingHistoryAccessor(200, AppContext.BrowseHistoryDatabase);
        }

        /// <summary>
        ///     Write Proxy-Auto-Configuration file to ..\{Directory to Pixeval.dll}\Resource\pixeval_pac.pac,
        ///     this method is for login usage only, USE AT YOUR OWN RISK
        /// </summary>
        private static async Task WritePac()
        {
            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("function FindProxyForURL(url, host) {");
            // only *.pixiv.net will request bypass proxy
            scriptBuilder.AppendLine("    if (shExpMatch(host, \"*.pixiv.net\")) {");
            scriptBuilder.AppendLine("        return 'PROXY 127.0.0.1:1234';");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("    return \"DIRECT\";");
            scriptBuilder.AppendLine("}");
            await File.WriteAllTextAsync(Path.Combine(AppContext.ResourceFolder, "pixeval_pac.pac"),
                scriptBuilder.ToString());
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            PluggableProtocolListener.StopServer();
            CertificateManager.GetFakeCaRootCertificate().Dispose();
            CertificateManager.GetFakeServerCertificate().Dispose();
            await Settings.Global.Store();
            if (Session.Current != null && !Session.Current.AccessToken.IsNullOrEmpty() &&
                !Session.Current.PhpSessionId.IsNullOrEmpty())
                await Session.Current.Store();
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

            AppContext.DefaultCacheProvider.Clear();
            base.OnExit(e);
        }
    }
}