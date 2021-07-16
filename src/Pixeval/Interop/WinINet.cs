using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

// From https://github.com/shadowsocks/shadowsocks-windows/blob/main/Shadowsocks.WPF/Services/SystemProxy/WinINet.cs
namespace Pixeval.Interop
{
    #region Windows API data structure definition

    public enum InternetOptions
    {
        Refresh = 37,
        SettingsChanged = 39,
        PerConnectionOption = 75,
        ProxySettingChanged = 95,
    }

    public enum InternetPerConnectionOptionEnum
    {
        Flags = 1,
        ProxyServer = 2,
        ProxyBypass = 3,
        AutoConfigUrl = 4,
        AutoDiscovery = 5,
        AutoConfigSecondaryUrl = 6,
        AutoConfigReloadDelay = 7,
        AutoConfigLastDetectTime = 8,
        AutoConfigLastDetectUrl = 9,
        FlagsUi = 10,
    }

    [Flags]
    public enum InternetPerConnectionFlags
    {
        Direct = 0x01,
        Proxy = 0x02,
        AutoProxyUrl = 0x04,
        AutoDetect = 0x08,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InternetPerConnectionOptionUnion : IDisposable
    {
        [FieldOffset(0)]
        public int dwValue;

        [FieldOffset(0)]
        public IntPtr pszValue;

        [FieldOffset(0)]
        public FILETIME ftValue;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pszValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszValue);
                    pszValue = IntPtr.Zero;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InternetPerConnectionOption
    {
        public int dwOption;
        public InternetPerConnectionOptionUnion Value;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct InternetPerConnectionOptionList : IDisposable
    {
        public int Size;
        public IntPtr Connection;
        public int OptionCount;
        public int OptionError;
        public IntPtr pOptions;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Connection);
                    Connection = IntPtr.Zero;
                }

                if (pOptions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pOptions);
                    pOptions = IntPtr.Zero;
                }
            }
        }
    }

    #endregion

    public class WinINetSetting
    {
        public InternetPerConnectionFlags Flags = InternetPerConnectionFlags.Direct;
        public string ProxyServer = string.Empty;
        public string ProxyBypass = string.Empty;
        public string AutoConfigUrl = string.Empty;
    }

    public class WinINet
    {
        private static readonly WinINetSetting? InitialSetting;

        public static bool Operational { get; set; } = true;

        static WinINet()
        {
            try
            {
                InitialSetting = Query();
            }
            catch (Exception e)
            {
                if (e is not DllNotFoundException)
                {
                    throw;
                }

                Operational = false;
            }
        }

        public static void ProxyGlobal(string server, string bypass)
        {
            var options = new List<InternetPerConnectionOption>
            {
                GetOption(InternetPerConnectionOptionEnum.Flags, InternetPerConnectionFlags.Proxy | InternetPerConnectionFlags.Direct),
                GetOption(InternetPerConnectionOptionEnum.ProxyServer, server),
                GetOption(InternetPerConnectionOptionEnum.ProxyBypass, bypass),
            };
            Exec(options);
        }

        public static void ProxyPac(string url)
        {
            var options = new List<InternetPerConnectionOption>
            {
                GetOption(InternetPerConnectionOptionEnum.Flags, InternetPerConnectionFlags.AutoProxyUrl | InternetPerConnectionFlags.Direct),
                GetOption(InternetPerConnectionOptionEnum.AutoConfigUrl, url),
            };
            Exec(options);
        }

        public static void Direct()
        {
            var options = new List<InternetPerConnectionOption>
            {
                GetOption(InternetPerConnectionOptionEnum.Flags, InternetPerConnectionFlags.Direct),
            };
            Exec(options);
        }

        public static void Restore()
        {
            Set(InitialSetting!);
        }

        public static void Set(WinINetSetting setting)
        {
            var options = new List<InternetPerConnectionOption>
            {
                GetOption(InternetPerConnectionOptionEnum.Flags, setting.Flags),
                GetOption(InternetPerConnectionOptionEnum.ProxyServer, setting.ProxyServer),
                GetOption(InternetPerConnectionOptionEnum.ProxyBypass, setting.ProxyBypass),
                GetOption(InternetPerConnectionOptionEnum.AutoConfigUrl, setting.AutoConfigUrl),
            };
            Exec(options);
        }

        public static void Reset()
        {
            Set(new WinINetSetting());
        }

        #region Windows API wrapper

        public static WinINetSetting Query()
        {
            if (!Operational)
            {
                return new WinINetSetting();
            }

            var options = new List<InternetPerConnectionOption>
            {
                new() {dwOption = (int) InternetPerConnectionOptionEnum.FlagsUi},
                new() {dwOption = (int) InternetPerConnectionOptionEnum.ProxyServer},
                new() {dwOption = (int) InternetPerConnectionOptionEnum.ProxyBypass},
                new() {dwOption = (int) InternetPerConnectionOptionEnum.AutoConfigUrl},
            };

            var (unmanagedList, listSize) = PrepareOptionList(options);
            var ok = InternetQueryOption(IntPtr.Zero, (int) InternetOptions.PerConnectionOption, unmanagedList, ref listSize);
            if (!ok)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var proxy = new WinINetSetting();

            var ret = Marshal.PtrToStructure<InternetPerConnectionOptionList>(unmanagedList);
            var p = ret.pOptions;
            var nOption = ret.OptionCount;
            var outOptions = new List<InternetPerConnectionOption>();
            for (var i = 0; i < nOption; i++)
            {
                var o = Marshal.PtrToStructure<InternetPerConnectionOption>(p);
                outOptions.Add(o);
                p += Marshal.SizeOf(o);
            }

            foreach (var o in outOptions)
            {
                switch ((InternetPerConnectionOptionEnum) o.dwOption)
                {
                    case InternetPerConnectionOptionEnum.FlagsUi:
                    case InternetPerConnectionOptionEnum.Flags:
                        proxy.Flags = (InternetPerConnectionFlags) o.Value.dwValue;
                        break;
                    case InternetPerConnectionOptionEnum.AutoConfigUrl:
                        proxy.AutoConfigUrl = Marshal.PtrToStringAuto(o.Value.pszValue) ?? "";
                        break;
                    case InternetPerConnectionOptionEnum.ProxyBypass:
                        proxy.ProxyBypass = Marshal.PtrToStringAuto(o.Value.pszValue) ?? "";
                        break;
                    case InternetPerConnectionOptionEnum.ProxyServer:
                        proxy.ProxyServer = Marshal.PtrToStringAuto(o.Value.pszValue) ?? "";
                        break;
                    case InternetPerConnectionOptionEnum.AutoDiscovery:
                    case InternetPerConnectionOptionEnum.AutoConfigSecondaryUrl:
                    case InternetPerConnectionOptionEnum.AutoConfigReloadDelay:
                    case InternetPerConnectionOptionEnum.AutoConfigLastDetectTime:
                    case InternetPerConnectionOptionEnum.AutoConfigLastDetectUrl:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return proxy;
        }

        private static InternetPerConnectionOption GetOption(
            InternetPerConnectionOptionEnum option,
            InternetPerConnectionFlags flag
        )
        {
            return new()
            {
                dwOption = (int) option,
                Value =
                {
                    dwValue = (int) flag,
                }
            };
        }

        private static InternetPerConnectionOption GetOption(
            InternetPerConnectionOptionEnum option,
            string param
        )
        {
            return new()
            {
                dwOption = (int) option,
                Value =
                {
                    pszValue = Marshal.StringToCoTaskMemAuto(param),
                }
            };
        }

        private static (IntPtr, int) PrepareOptionList(IReadOnlyCollection<InternetPerConnectionOption> options)
        {
            var len = options.Sum(Marshal.SizeOf);

            var buf = Marshal.AllocCoTaskMem(len);
            var cur = buf;

            foreach (var o in options)
            {
                Marshal.StructureToPtr(o, cur, false);
                cur += Marshal.SizeOf(o);
            }

            var optionList = new InternetPerConnectionOptionList
            {
                pOptions = buf,
                OptionCount = options.Count,
                Connection = IntPtr.Zero,
                OptionError = 0,
            };
            var listSize = Marshal.SizeOf(optionList);
            optionList.Size = listSize;

            var unmanagedList = Marshal.AllocCoTaskMem(listSize);
            Marshal.StructureToPtr(optionList, unmanagedList, true);
            return (unmanagedList, listSize);
        }

        private static void ClearOptionList(IntPtr list)
        {
            var l = Marshal.PtrToStructure<InternetPerConnectionOptionList>(list);
            Marshal.FreeCoTaskMem(l.pOptions);
            Marshal.FreeCoTaskMem(list);
        }

        private static void Exec(IReadOnlyCollection<InternetPerConnectionOption> options)
        {
            if (!Operational)
            {
                return;
            }

            var (unmanagedList, listSize) = PrepareOptionList(options);

            var ok = InternetSetOption(
                IntPtr.Zero,
                (int) InternetOptions.PerConnectionOption,
                unmanagedList,
                listSize
            );

            if (!ok)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            ClearOptionList(unmanagedList);
            ok = InternetSetOption(
                IntPtr.Zero,
                (int) InternetOptions.ProxySettingChanged,
                IntPtr.Zero,
                0
            );
            if (!ok)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            ok = InternetSetOption(
                IntPtr.Zero,
                (int) InternetOptions.Refresh,
                IntPtr.Zero,
                0
            );
            if (!ok)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetQueryOption(IntPtr hInternet, uint dwOption, IntPtr lpBuffer, ref int lpdwBufferLength);

        #endregion
    }
}