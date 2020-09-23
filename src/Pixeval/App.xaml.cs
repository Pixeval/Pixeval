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

#if RELEASE
using Pixeval.Objects.Exceptions.Logger;
#endif
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using Pixeval.Core;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Pixeval.Persisting.WebApi;

namespace Pixeval
{
    public partial class App
    {
        public App()
        {
            if (Dispatcher != null) Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if RELEASE
            ExceptionDumper.WriteException(e);
#elif DEBUG
            throw e;
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
            PortScan(out var proxy, out var pac);
            AppContext.ProxyPort = proxy;
            AppContext.PacPort = pac;
            await WritePac();
            CefSharpInitialize();

            // These initializations handle USER SESSION AND PIXEVAL UPDATE
            await RestoreSettings();
            await CheckUpdate();

            // These initializations are for PROCESS COMMUNICATION AND PLUGGABLE PROTOCOL
            await InstallPluggableProtocolHandler();
            CreatePluggableProtocolRegistry();
            PluggableProtocolListener.StartServer();


            if (e.Args.Any()) await PluggableProtocolParser.Parse(e.Args[0]);
            base.OnStartup(e);
        }

        private static void PortScan(out int proxy, out int pac)
        {
            var unavailablePorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(t => t.LocalEndPoint.Port).ToArray();

            var rd = new Random();
            proxy = RandPorts();
            pac = RandPorts();
            while (Array.BinarySearch(unavailablePorts, proxy) >= 0) proxy = RandPorts();

            while (Array.BinarySearch(unavailablePorts, pac) >= 0) pac = RandPorts();

            int RandPorts()
            {
                return rd.Next(3000, 65536);
            }
        }

        private static async Task InstallPluggableProtocolHandler()
        {
            var files = Directory.GetFiles(AppContext.InterchangeFolder);
            if (!(files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] == "Pixeval.Interchange.runtimeconfig.json") && files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] == "Pixeval.Interchange.exe") && files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] == "Pixeval.Interchange.dll")))
            {
                foreach (var file in files) File.Delete(file);
                if (GetResourceStream(new Uri("pack://application:,,,/Pixeval;component/Resources/interchange.zip")) is { } streamResourceInfo)
                {
                    var interchange = Path.Combine(AppContext.InterchangeFolder, "interchange.zip");
                    await using (var fs = new FileStream(interchange, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        await streamResourceInfo.Stream.CopyToAsync(fs);
                    }

                    ZipFile.ExtractToDirectory(interchange, AppContext.InterchangeFolder, true);
                    File.Delete(Path.Combine(AppContext.InterchangeFolder, "interchange.zip"));
                }
            }
        }

        private static void WriteToCurrentUserPathVariable()
        {
            var pathOld = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (pathOld == null)
            {
                Environment.SetEnvironmentVariable("PATH", "", EnvironmentVariableTarget.User);
                pathOld = "";
            }

            var paths = pathOld.Split(';').ToList();
            var location = Path.GetDirectoryName(typeof(App).Assembly.Location);
            if (paths.All(p => p != location))
            {
                paths.RemoveAll(p => p.ToLower().Contains("pixeval"));
                var newPath = paths.Join(';') + ';' + location;
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
            }
        }

        private static void InitializeFolders()
        {
            Directory.CreateDirectory(AppContext.ProjectFolder);
            Directory.CreateDirectory(AppContext.SettingsFolder);
            Directory.CreateDirectory(AppContext.ExceptionReportFolder);
            Directory.CreateDirectory(AppContext.ResourceFolder);
            Directory.CreateDirectory(AppContext.PermanentlyFolder);
            Directory.CreateDirectory(AppContext.InterchangeFolder);
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
                shellKey.SetValue("", $"{Path.Combine(AppContext.InterchangeFolder, "Pixeval.Interchange.exe")} %1");
            }

            regKey?.Dispose();
        }

        private static void CheckMultipleProcess()
        {
            if (Process.GetProcessesByName(AppContext.AppIdentifier).Length > 1)
            {
                MessageBox.Show(AkaI18N.MultiplePixevalInstanceDetected, AkaI18N.MultiplePixevalInstanceDetectedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }
        }

        private static void CheckCppRedistributable()
        {
            if (!CppRedistributableInstalled())
            {
                MessageBox.Show(AkaI18N.CppRedistributableRequired, AkaI18N.CppRedistributableRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Clipboard.SetDataObject("https://support.microsoft.com/zh-cn/help/2977003/the-latest-supported-visual-c-downloads");
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

        /// <summary>
        ///     Check if the required Visual C++ Redistributable is installed on the computer
        /// </summary>
        /// <returns>Cpp redistributable is installed</returns>
        private static bool CppRedistributableInstalled()
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
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
            Cef.Initialize(new CefSettings {CefCommandLineArgs = {{"proxy-pac-url", $"http://127.0.0.1:{AppContext.PacPort}/pixeval_pac.pac"}}}, true, browserProcessHandler: null);
        }

        private static async Task InstallFakeCaCertificate()
        {
            var certificateManager = new CertificateManager(await CertificateManager.GetFakeCaRootCertificate());
            if (!certificateManager.Query(StoreName.Root, StoreLocation.CurrentUser))
            {
                if (MessageBox.Show(AkaI18N.CertificateInstallationIsRequired, AkaI18N.CertificateInstallationIsRequiredTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    certificateManager.Install(StoreName.Root, StoreLocation.CurrentUser);
                else
                    Environment.Exit(-1);
            }
        }

        private static async Task RestoreSettings()
        {
            await Settings.Restore();
            BrowsingHistoryAccessor.GlobalLifeTimeScope = new BrowsingHistoryAccessor(200, AppContext.BrowseHistoryDatabase);
        }

        /// <summary>
        ///     Write Proxy-Auto-Configuration file to ..\{Directory to Pixeval.dll}\Resource\pixeval_pac.pac,
        ///     this method is for login usage only, USE AT YOUR OWN RISK
        /// </summary>
        private static async Task WritePac()
        {
            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("function FindProxyForURL(url, host) {");
            scriptBuilder.AppendLine("    if (shExpMatch(host, \"*.pixiv.net\")) {");
            scriptBuilder.AppendLine($"        return 'PROXY 127.0.0.1:{AppContext.ProxyPort}';");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("    return \"DIRECT\";");
            scriptBuilder.AppendLine("}");
            await File.WriteAllTextAsync(Path.Combine(AppContext.ResourceFolder, "pixeval_pac.pac"), scriptBuilder.ToString());
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            CertificateManager.GetFakeCaRootCertificate().Dispose();
            CertificateManager.GetFakeServerCertificate().Dispose();
            await Settings.Global.Store();
            if (Session.Current != null && !Session.Current.AccessToken.IsNullOrEmpty() && !Session.Current.PhpSessionId.IsNullOrEmpty()) await Session.Current.Store();
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