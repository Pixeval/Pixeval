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
#elif DEBUG
using System.Globalization;
#endif
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
using Pixeval.UI;

namespace Pixeval
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }
        //public App()
        //{
        //if (Dispatcher != null) Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        //AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        //    DispatcherOnUnhandledException((Exception)args.ExceptionObject);
        //TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        //}

        //        private static void DispatcherOnUnhandledException(Exception e)
        //        {
        //#if RELEASE
        //            ExceptionDumper.WriteException(e);
        //#elif DEBUG
        //            throw e;
        //#endif
        //        }

        

        protected override async void OnStartup(StartupEventArgs e)
        {

            // These initializations ensure Pixeval to run properly, they MUST be initialized before everything start
            WriteToCurrentUserPathVariable();
            InitializeFolders();
            CheckMultipleProcess();
            //await InstallFakeCaCertificate();

            // These initializations are for WEB API LOGIN
            PortScan(out var proxy, out var pac);
            AppContext.ProxyPort = proxy;
            AppContext.PacPort = pac;
            await WritePac();
            CefSharpInitialize();

            // These initializations handle USER SESSION AND PIXEVAL UPDATE
            await RestoreSettings();

            // These initializations are for PROCESS COMMUNICATION AND PLUGGABLE PROTOCOL
            await InstallPluggableProtocolHandler();
            CreatePluggableProtocolRegistry();
            PluggableProtocolListener.StartServer();


            if (e.Args.Any()) await PluggableProtocolParser.Parse(e.Args[0]);
            base.OnStartup(e);
        }

        private static void PortScan(out int proxy, out int pac)
        {
            var unavailablePorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                .Select(t => t.LocalEndPoint.Port).ToArray();

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
            if (!(files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] ==
                                "Pixeval.Interchange.runtimeconfig.json") &&
                files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] == "Pixeval.Interchange.exe") &&
                files.Any(f => f[(f.LastIndexOf(@"\", StringComparison.Ordinal) + 1)..] == "Pixeval.Interchange.dll")))
            {
                foreach (var file in files) File.Delete(file);
                if (GetResourceStream(
                        new Uri("pack://application:,,,/Pixeval;component/Resources/interchange.zip")) is { }
                    streamResourceInfo)
                {
                    var interchange = Path.Combine(AppContext.InterchangeFolder, "interchange.zip");
                    await using (var fs =
                        new FileStream(interchange, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
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
                shellKey.SetValue("",
                                  $"{Path.Combine(AppContext.InterchangeFolder, "Pixeval.Interchange.exe")} %1");
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

        private static void CefSharpInitialize()
        {
            Cef.Initialize(new CefSettings
            {
                CefCommandLineArgs =
                {
                    {"proxy-pac-url", $"http://127.0.0.1:{AppContext.PacPort}/pixeval_pac.pac"}
                }
            }, true, browserProcessHandler: null);
        }

        private static async Task RestoreSettings()
        {
            await Settings.Restore();
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
            scriptBuilder.AppendLine("    if (shExpMatch(host, \"*.pixiv.net\")) {");
            scriptBuilder.AppendLine($"        return 'PROXY 127.0.0.1:{AppContext.ProxyPort}';");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("    return \"DIRECT\";");
            scriptBuilder.AppendLine("}");
            await File.WriteAllTextAsync(Path.Combine(AppContext.ResourceFolder, "pixeval_pac.pac"),
                                         scriptBuilder.ToString());
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            CertificateManager.GetFakeCaRootCertificate().Dispose();
            CertificateManager.GetFakeServerCertificate().Dispose();
            await Settings.Global.Store();
            if (Session.Current != null &&
                !Session.Current.AccessToken.IsNullOrEmpty() &&
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
            base.OnExit(e);
        }

        
    }
}
