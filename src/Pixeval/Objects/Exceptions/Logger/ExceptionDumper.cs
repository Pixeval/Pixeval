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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Pixeval.Wpf.Objects.Primitive;
using Refit;

namespace Pixeval.Wpf.Objects.Exceptions.Logger
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Memorystatusex
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }


    public class ExceptionDumper
    {

        public static async void WriteException(Exception e)
        {
            using var semaphore = new SemaphoreSlim(1);
            await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
            ApplicationLog stack;
            try
            {
                stack = new ApplicationLogStack(AppContext.AppIdentifier, ".NET Runtime").GetFirst();
            }
            catch
            {
                stack = null;
            }

            string exceptionMessage = e is ApiException exception
                ? exception.Content + Environment.NewLine + exception
                : e.ToString();
            var sb = new StringBuilder();
            sb.AppendLine(@"Pixeval - A Strong, Fast and Flexible Pixiv Client");
            sb.AppendLine(@"Copyright (C) 2019-2020 Dylech30th");
            sb.AppendLine();
            sb.AppendLine(@"We have encountered an unrecoverable problem. A dump file with error snapshot has been created.");
            sb.AppendLine(@"In order to help with diagnosis and debug, Pixeval will collect some information contains: ");
            sb.AppendLine(@"    ¡¤ Computer Architecture");
            sb.AppendLine(@"    ¡¤ Operating System");
            sb.AppendLine(@"    ¡¤ Event Log");
            sb.AppendLine(@"    ¡¤ Exception Message");
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
            sb.AppendLine($"        Total Available RAM: {GetAvailableMemory()} MB");
            sb.AppendLine(@"    End Computer Architecture");
            sb.AppendLine();
            sb.AppendLine(@"    Begin Operating System");
            sb.AppendLine($"        OS Version String: {Environment.OSVersion.VersionString}");
            sb.AppendLine($"        OS Version {Environment.OSVersion.Version}");
            sb.AppendLine($"        Service Pack Version: {(Environment.OSVersion.ServicePack.IsNullOrEmpty() ? "Not Installed" : "Environment.OSVersion.ServicePack")}");
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
            try
            {
                await File.WriteAllTextAsync(Path.Combine(AppContext.ExceptionReportFolder, $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.txt".Replace("/", "-").Replace(":", "-")), sb.ToString());
            }
            catch
            {
                // ignore
            }
        }

        private static string FormatMultilineData(string data, int indent)
        {
            string indentation = new string(' ', indent * 4);
            return data.Split('\n').Select(s => indentation + s).Join('\n');
        }

        private static string GetCppRedistributableVersion()
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            return key == null ? "Not Installed" : key.GetValue("Bld").ToString();
        }

        private static ulong GetTotalInstalledMemory()
        {
            var meminfo = new Memorystatusex();
            int temp;

            meminfo.dwLength = 64; //此方法为手动Hack，按照填充规则计算大小，祝我好运，希望有更好的办法（但是Unsafe的sizeof算符简直令人无力吐槽）
            temp = GlobalMemoryStatusEx(ref meminfo);//实践证明，必须有人接收返回值，否则会报错

            return meminfo.ullTotalPhys / 1024 / 1024 / 1024;


        }

        private static ulong GetAvailableMemory()
        {
            var meminfo = new Memorystatusex();
            int temp;

            meminfo.dwLength = 64; //此方法为手动Hack，按照填充规则计算大小，祝我好运
            temp = GlobalMemoryStatusEx(ref meminfo);//实践证明，必须有人接收返回值，否则会报错
            return meminfo.ullAvailPhys / 1024 / 1024;

        }

        [DllImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx", CallingConvention = CallingConvention.StdCall)]//此处一定要用Ex，否则内存计算不全
        private static extern int GlobalMemoryStatusEx(ref Memorystatusex lpBuffer);
    }
}
