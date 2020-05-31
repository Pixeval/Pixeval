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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Pixeval.Objects.Primitive;
using Refit;

namespace Pixeval.Objects.Exceptions.Logger
{
    public class ExceptionDumper
    {
        public static async void WriteException(Exception e)
        {
            ApplicationLog stack;
            try
            {
                stack = new ApplicationLogStack(AppContext.AppIdentifier, ".NET Runtime").GetFirst();
            }
            catch
            {
                stack = null;
            }

            var exceptionMessage = e is ApiException exception
                ? exception.Content + Environment.NewLine + exception
                : e.ToString();
            var sb = new StringBuilder();
            sb.AppendLine(@"Pixeval - A Strong, Fast and Flexible Pixiv Client");
            sb.AppendLine(@"Copyright (C) 2019-2020 Dylech30th");
            sb.AppendLine();
            sb.AppendLine(
                @"We have encountered an unrecoverable problem. A dump file with error snapshot has been created.");
            sb.AppendLine(
                @"In order to help with diagnosis and debug, Pixeval will collect some information contains: ");
            sb.AppendLine(@"    ， Computer Architecture");
            sb.AppendLine(@"    ， Operating System");
            sb.AppendLine(@"    ， Event Log");
            sb.AppendLine(@"    ， Exception Message");
            sb.AppendLine();
            sb.AppendLine(@"Begin Dump Information");
            sb.AppendLine($"    Pixeval Version: {AppContext.CurrentVersion}");
            sb.AppendLine($"    Creation: {DateTime.Now}");
            sb.AppendLine(@"End Dump Information");
            sb.AppendLine();
            sb.AppendLine(@"Begin Debugging Information Collection");
            sb.AppendLine(@"    Begin Computer Architecture");
            sb.AppendLine($"        CPU Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")}");
            sb.AppendLine($"        Is 64-bit Platform: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"        Is 64-bit Process: {Environment.Is64BitProcess}");
            sb.AppendLine($"        Total Installed RAM: {GetTotalInstalledMemory()} GB");
            sb.AppendLine($"        Total Available RAM: {await GetAvailableMemory()} MB");
            sb.AppendLine(@"    End Computer Architecture");
            sb.AppendLine();
            sb.AppendLine(@"    Begin Operating System");
            sb.AppendLine($"        OS Version String: {Environment.OSVersion.VersionString}");
            sb.AppendLine($"        OS Version {Environment.OSVersion.Version}");
            sb.AppendLine(
                $"        Service Pack Version: {(Environment.OSVersion.ServicePack.IsNullOrEmpty() ? "Not Installed" : "Environment.OSVersion.ServicePack")}");
            sb.AppendLine($"        Visual C++ Redistributable Version: {GetCppRedistributableVersion()}");
            sb.AppendLine(@"    End Operating System");
            sb.AppendLine();
            sb.AppendLine(@"    Begin Event Log");
            sb.AppendLine(@"        Represents the latest application event log related to Pixeval");
            sb.AppendLine($"        Creation: {stack?.Creation}");
            sb.AppendLine(@"        Data:");
            sb.AppendLine(FormatMultilineData(stack?.Data, 3));
            sb.AppendLine(@"    End Event Log");
            sb.AppendLine();
            sb.AppendLine(@"    Begin Exception Log");
            sb.AppendLine(@"        Exception: ");
            sb.AppendLine(FormatMultilineData(exceptionMessage, 3));
            sb.AppendLine("     End Exception Log");
            sb.AppendLine(@"End Debugging Information Collection");
            await File.WriteAllTextAsync(
                Path.Combine(AppContext.ExceptionReportFolder,
                             $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.txt".Replace("/", "-")
                                 .Replace(":", "-")),
                sb.ToString());
        }

        private static string FormatMultilineData(string data, int indent)
        {
            var indentation = new string(' ', indent * 4);
            return data.Split('\n').Select(s => indentation + s).Join('\n');
        }

        private static string GetCppRedistributableVersion()
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            return key == null ? "Not Installed" : key.GetValue("Bld").ToString();
        }

        private static long GetTotalInstalledMemory()
        {
            GetPhysicallyInstalledSystemMemory(out var l);
            return l / 1024 / 1024;
        }

        private static Task<long> GetAvailableMemory()
        {
            return Task.Run(() => (long) new PerformanceCounter("Memory", "Available MBytes").NextValue());
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
    }
}